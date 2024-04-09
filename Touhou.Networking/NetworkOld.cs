using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;
using Touhou.Objects;

namespace Touhou.Networking;

public class NetworkOld {



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

    private Queue<byte[]> packetSendingBuffer = new();
    private Queue<byte[]> packetOutgoingQueue = new();

    private MemoryStream sendingStream = new();
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




    private Dictionary<PacketType, LinkedList<NetworkSubscriber>> subscribers = new();



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
        TimeOffset = 0L;
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

    public unsafe void Send(PacketType type, params object[] values) {

        if (!isConnected) return;

        int size = 0;

        Log.Info($"Sending {type}, values: {values.Length}");
        foreach (var value in values) {
            int valueSize = Marshal.SizeOf(value);
            size += valueSize;

            Log.Info($"{value.GetType}: {valueSize}");
        }

        var data = new byte[sizeof(int) + sizeof(PacketType) + size];

        fixed (byte* dataPtr = data) {
            Marshal.StructureToPtr(size, (nint)dataPtr, false);
            Marshal.StructureToPtr((byte)type, (nint)dataPtr + sizeof(int), false);
            if (size > 0) Marshal.StructureToPtr(values, (nint)dataPtr + sizeof(int) + sizeof(PacketType), false);
        }

        packetSendingBuffer.Enqueue(data);
        totalUniquePacketsSent++;
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

        Log.Info($"Received data, size: {data.Length}");

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



        int numToRead = totalTheySent - totalUniquePacketsReceived;

        Log.Info($"their time: {theirTime}, their perceived latency: {theirPerceivedLatency}, total they sent: {totalTheySent}, total they received: {totalTheyReceived}, num packets: {numPackets}");
        Log.Info($"total we sent: {totalUniquePacketsSent}, total we received: {totalUniquePacketsReceived}");
        // get size of packet but dont process it if the index is negative

        int offset = 28;
        for (int i = numToRead - numPackets; i < numToRead; i++) {

            int size = BitConverter.ToInt32(data, offset);

            Log.Info(i);

            if (i < 0) {
                offset += sizeof(Int32) + sizeof(Byte) + size;
                continue;
            }

            var packetData = new byte[size];

            offset += sizeof(Int32);
            var packetType = (PacketType)data[offset];

            offset += sizeof(Byte);
            System.Buffer.BlockCopy(data, offset, packetData, 0, size);

            Log.Info($"size: {size}, type: {packetType}, data: {string.Join(", ", packetData)}");

            var packet = new Packet(packetType, packetData);

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

                Send(PacketType.LatencyCorrection, PerceivedLatency);
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

        // append packets
        foreach (var packet in packetOutgoingQueue) {
            data = data.Concat(packet);
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

        if (packetSendingBuffer.Count > 0) {

            while (packetSendingBuffer.Count > 0) {
                packetOutgoingQueue.Enqueue(packetSendingBuffer.Dequeue());
            }

            SendInternal();

        } else {
            if (isConnected && Game.Time - lastSentTime >= sendRetryInterval) SendInternal();
        }


    }
}