using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Steamworks;
using Steamworks.Data;

namespace Touhou.Networking;

public class Network {



    public bool IsConnected { get => isConnected; }


    public Time Time { get => Game.Time - connectionTime + TimeOffset; }


    public Time PerceivedLatency { get; private set; }
    public Time Ping { get => PerceivedLatency * 2; }


    public Time TheirPerceivedLatency { get; private set; }
    public Time TheirPing { get => TheirPerceivedLatency * 2; }


    public Time TimeOffset { get; set; }


    public event Action<Packet, IPEndPoint> PacketReceived;
    public event Action<Time> DataReceived;


    private UdpClient udpClient;


    //private Stopwatch packetRetryTimer = new();
    //private Stopwatch disconnectionTimer = new();

    private Time lastSentTime;
    private Time lastReceivedTime;
    private Time sendRetryInterval = Time.InMilliseconds(100);

    private int totalUniquePacketsSent = 0;
    private int totalUniquePacketsReceived = 0;

    private Queue<float> pingSamples = new();

    private Queue<Packet> packetOutgoingQueue = new();
    private Queue<(int Size, Time Time)> sizeTimeStamps = new();
    private int dataUsage;

    private Time connectionTime;
    private Queue<Time> correctionSamples = new();
    private const int maxLatencyCorrectionSamples = 100;
    private bool isLatencyCorrectionProfiling;
    private int profilingCount;

    private FixedQueue<Time> latencySamples = new(1000);



    private bool forceFlush;
    private SteamSocketManager socketManager;
    private SteamConnectionManager connectionManager;
    private bool isConnected;



    public void Host(int port) {

        if (isConnected || (udpClient?.Client.IsBound ?? false)) return;

        udpClient = new UdpClient(port);
    }

    public void HostSteam() {
        if (isConnected) return;

        socketManager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>();

        connectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(SteamClient.SteamId);
        connectionManager.DataReceived += receivedData;
        connectionManager.Disconnected += () => Game.Scenes.Current?.OnDisconnect();

        isConnected = true;
    }



    public void Connect(IPEndPoint endPoint) {
        if (isConnected) return;

        udpClient ??= new UdpClient();
        udpClient.Connect(endPoint);

        connectionTime = Game.Time;
        lastReceivedTime = Game.Time;

        isConnected = true;
    }

    public void ConnectSteam(SteamId id) {
        if (isConnected) return;

        connectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(id);
        connectionManager.DataReceived += receivedData;
        connectionManager.Disconnected += () => Game.Scenes.Current?.OnDisconnect();

        connectionTime = Game.Time;
        lastReceivedTime = Game.Time;

        isConnected = true;
    }



    public void Disconnect() {
        if (!isConnected) return;

        udpClient?.Close();
        udpClient?.Dispose();
        udpClient = null;

        packetOutgoingQueue.Clear();
        totalUniquePacketsSent = 0;
        totalUniquePacketsReceived = 0;

        pingSamples.Clear();
        sizeTimeStamps.Clear();

        dataUsage = 0;

        isConnected = false;
    }

    public void DisconnectSteam() {
        if (!isConnected) return;

        connectionManager?.Close();
        socketManager?.Close();

        packetOutgoingQueue.Clear();
        totalUniquePacketsSent = 0;
        totalUniquePacketsReceived = 0;

        pingSamples.Clear();
        sizeTimeStamps.Clear();

        dataUsage = 0;

        isConnected = false;
    }

    public void Send(Packet packet) {
        if (!isConnected) return;
        packetOutgoingQueue.Enqueue(packet);
        totalUniquePacketsSent++;
        forceFlush = true;
    }

    public void Update() {

        if (isConnected && Game.Time > lastReceivedTime + Time.InSeconds(3)) {
            Game.Scenes.Current?.OnDisconnect();
        }

        // steam
        socketManager?.Receive();
        connectionManager?.Receive();



        // direct
        while (udpClient?.Available > 0) {

            var theirEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var data = udpClient.Receive(ref theirEndPoint);
            receivedData(data, theirEndPoint);
        }
    }

    private void receivedData(byte[] data, IPEndPoint endPoint) {



        lastReceivedTime = Game.Time;

        dataUsage = CalculateDataUsage(data.Length);

        // _______[ data format ]_______
        // their time              [offset: 0,  size: 8]
        // their percieved latency [offset: 8, size: 8]
        // total sent              [offset: 16,  size: 4]
        // total received          [offset: 20, size: 4]
        // num packets             [offset: 24, size: 4]

        // packet meta block       [offset: 28, size: 8 * n]
        // packet block            [position: ~,  size: ~]

        // _______[ packet meta ]_______
        // packet offset           [offset: 0, size: 4]
        // packet size             [offset: 4, size: 4]

        // _______[ packet ]_______
        // type                    [offset: 0, size: 1]
        // data                    [offset: 1, size: ~]

        Time theirTime = BitConverter.ToInt64(data, 0);
        Time theirPerceivedLatency = BitConverter.ToInt64(data, 8);
        int totalTheySent = BitConverter.ToInt32(data, 16);
        int totalTheyReceived = BitConverter.ToInt32(data, 20);
        int numPackets = BitConverter.ToInt32(data, 24);

        TheirPerceivedLatency = theirPerceivedLatency;

        SampleLatency(theirTime, theirPerceivedLatency);
        DequeueOutgoingPackets(totalTheyReceived);

        int packetMetaBlockOffset = 28;
        int packetBlockOffset = packetMetaBlockOffset + 8 * numPackets;

        var individualPacketOffsets = new int[numPackets];
        var individualPacketSizes = new int[numPackets];


        for (int i = 0; i < numPackets; i++) {
            individualPacketOffsets[i] = BitConverter.ToInt32(data, packetMetaBlockOffset + 8 * i);
            individualPacketSizes[i] = BitConverter.ToInt32(data, packetMetaBlockOffset + 8 * i + 4);
        }

        int numToRead = totalTheySent - totalUniquePacketsReceived;


        for (int i = numPackets - numToRead; i < numPackets; i++) {
            var packetType = (PacketType)data[packetBlockOffset + individualPacketOffsets[i]];

            Log.Info($"{packetType} packed received");

            var packet = new Packet(packetType);

            int offset = packetBlockOffset + individualPacketOffsets[i] + 1;
            int size = individualPacketSizes[i];

            packet.In(data, offset, size);

            totalUniquePacketsReceived++;

            if (packetType == PacketType.LatencyCorrection) {
                packet.Out(out Time theirLatency);


                var latencyDifference = Math.Abs(theirLatency - PerceivedLatency);
                var latencySign = Math.Sign(theirLatency - PerceivedLatency);

                var offsetChange = (Time)Math.Round(Math.Min(Math.Pow(latencyDifference * 0.05d, 2d), latencyDifference * 0.5d) * latencySign);

                //var offsetChange = (Time)Math.Round((PerceivedLatency - theirLatency) * 0.1d);
                Log.Info($"Adjusting network time offset: {offsetChange}μs");
                TimeOffset += offsetChange;

                isLatencyCorrectionProfiling = true;

                packet.ResetReadPosition();
            }

            PacketReceived?.Invoke(packet, endPoint);
        }

    }
    private void SampleLatency(Time theirTime, Time theirPerceivedLatency) {
        var latency = Time - theirTime;

        DataReceived?.Invoke(latency);


        latencySamples.Enqueue(latency);

        correctionSamples.Enqueue(Time - theirTime);

        while (correctionSamples.Count > maxLatencyCorrectionSamples) correctionSamples.Dequeue(); // dequeue old

        PerceivedLatency = GetAverageLatency(50);

        if (isLatencyCorrectionProfiling) {
            profilingCount++;

            if (profilingCount >= maxLatencyCorrectionSamples) {
                profilingCount = 0;
                isLatencyCorrectionProfiling = false;

                Send(new Packet(PacketType.LatencyCorrection).In(PerceivedLatency));
            }
        }

        // var offsetChange = (Time)Math.Round((PerceivedLatency - theirPerceivedLatency) * 0.01d);
        // Log.Info(offsetChange);
        // TimeOffset -= offsetChange;






        // pingSamples.Enqueue((Time - theirTime) / 500000f);
        // while (pingSamples.Count > 100) {
        //     pingSamples.Dequeue();
        // }
    }

    private void DequeueOutgoingPackets(int totalTheyReceived) {
        int numToDequeue = packetOutgoingQueue.Count - (totalUniquePacketsSent - totalTheyReceived);
        for (int i = 0; i < numToDequeue; i++) packetOutgoingQueue.Dequeue();
    }

    private void SendInternal() {

        var data = new byte[0]
        .Concat(BitConverter.GetBytes(Time))
        .Concat(BitConverter.GetBytes(PerceivedLatency))
        .Concat(BitConverter.GetBytes(totalUniquePacketsSent))
        .Concat(BitConverter.GetBytes(totalUniquePacketsReceived))
        .Concat(BitConverter.GetBytes(packetOutgoingQueue.Count));

        // append packet meta
        int currentOffset = 0;
        foreach (var bufferedPacket in packetOutgoingQueue) {
            data = data
            .Concat(BitConverter.GetBytes(currentOffset))
            .Concat(BitConverter.GetBytes(bufferedPacket.Size));

            currentOffset += 1 + bufferedPacket.Size; //type + data
        }

        // append packets
        foreach (var bufferedPacket in packetOutgoingQueue) {
            data = data
            .Append((byte)bufferedPacket.Type)
            .Concat(bufferedPacket.Data);
        }

        connectionManager?.Connection.SendMessage(data.ToArray(), SendType.Unreliable);
        udpClient?.Send(data.ToArray());

        lastSentTime = Game.Time;
    }

    private int CalculateDataUsage(int size) {
        sizeTimeStamps.Enqueue((size, Game.Time));

        while (sizeTimeStamps.Count > 0) {
            if (Game.Time > sizeTimeStamps.Peek().Time + Time.InSeconds(1)) break; // break if first element is not older than 1s
            sizeTimeStamps.Dequeue();
        }

        return sizeTimeStamps.Sum(e => e.Size);
    }

    private Time GetAverageLatency(int numToPrune) {

        var samples = correctionSamples.ToList();

        Time total = default(Time);
        int count = samples.Count;
        foreach (var sample in samples) {
            total += sample;
        }

        if (count <= numToPrune) {
            return (long)Math.Round((double)total / count);
        }

        var prunedSamples = new HashSet<int>();

        for (int n = 0; n < numToPrune; n++) {
            var influences = new List<(double InfluenceAmount, int Index)>();

            double average = (double)total / count;

            for (int i = 0; i < samples.Count; i++) {
                if (prunedSamples.Contains(i)) continue;

                var sample = samples[i];

                double influenceAmount = Math.Abs(average - (double)(total - sample) / (count - 1));
                influences.Add((influenceAmount, i));
            }

            var largestInfluence = influences.MaxBy(e => e.InfluenceAmount);
            prunedSamples.Add(largestInfluence.Index);

            total -= samples[largestInfluence.Index];
            count--;
        }

        return (long)Math.Round((double)total / count);
    }

    public void StartLatencyCorrection() => isLatencyCorrectionProfiling = true;

    public void ResetPing() {
        correctionSamples.Clear();
    }

    public void Flush() {

        if (forceFlush) {
            SendInternal();
            forceFlush = false;
        } else {
            if (isConnected && Game.Time - lastSentTime >= sendRetryInterval) SendInternal();
        }


    }
}