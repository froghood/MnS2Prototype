using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using SFML.System;
using SFML.Graphics;

namespace Touhou.Net;

public class Network {



    public bool Connected { get => udpClient.Client.Connected; }

    public Time Time { get => Game.Time - connectionTime + TimeOffset; }

    public Time PerceivedLatency { get; private set; }
    public Time Ping { get => PerceivedLatency * 2; }

    public Time TheirPerceivedLatency { get; private set; }
    public Time TheirPing { get => TheirPerceivedLatency * 2; }



    //public float Ping { get => pingSamples.Sum() / pingSamples.Count; }
    public Time TimeOffset { get; set; }


    public event Action<Packet, IPEndPoint> PacketReceived;
    public event Action<Time> DataReceived;


    private UdpClient udpClient = new();


    //private Stopwatch packetRetryTimer = new();
    //private Stopwatch disconnectionTimer = new();

    private Time lastSentTime;
    private Time lastReceivedTime;
    private Time sendRetryInterval = Time.InMilliseconds(100);

    private int totalPacketsSent = 0;
    private int totalPacketsReceived = 0;

    private Queue<float> pingSamples = new();

    private Queue<Packet> packetOutgoingQueue = new();
    private Queue<Packet> packetArrivalQueue = new();
    private Queue<(int Size, Time Time)> sizeTimeStamps = new();
    private int dataUsage;

    private Time connectionTime;
    private Queue<Time> correctionSamples = new();
    private const int maxLatencyCorrectionSamples = 20;
    private bool isLatencyCorrectionProfiling;
    private int profilingCount;

    private FixedQueue<Time> latencySamples = new(1000);


    public void Host(int port) {
        if (udpClient.Client.IsBound) return;
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

        System.Console.WriteLine($"Hosting on port {port}");
    }

    public void Connect(IPEndPoint endPoint) {
        if (Connected) return;
        udpClient.Connect(endPoint);

        connectionTime = Game.Time;
        lastReceivedTime = Game.Time;

        //packetRetryTimer.Restart();
        //disconnectionTimer.Restart();

        System.Console.WriteLine($"Connecting to {endPoint}");
    }

    public void Disconnect() {
        if (!Connected) return;

        udpClient.Close();
        udpClient = new UdpClient();

        //packetRetryTimer.Reset();
        //disconnectionTimer.Reset();

        packetOutgoingQueue.Clear();
        totalPacketsSent = 0;
        totalPacketsReceived = 0;

        pingSamples.Clear();

        sizeTimeStamps.Clear();
        dataUsage = 0;
    }

    public void Send(Packet packet) {
        if (!Connected) return;
        packetOutgoingQueue.Enqueue(packet);
        totalPacketsSent++;
        SendInternal();
    }

    public void Update() {

        if (Connected && Game.Time > lastReceivedTime + Time.InSeconds(5)) {
            Game.Scenes.Current?.OnDisconnect();
        }

        while (udpClient.Available > 0) {

            lastReceivedTime = Game.Time;

            var theirEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var data = udpClient.Receive(ref theirEndPoint);

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

            int numToRead = totalTheySent - totalPacketsReceived;


            for (int i = numPackets - numToRead; i < numPackets; i++) {
                var packetType = (PacketType)data[packetBlockOffset + individualPacketOffsets[i]];

                System.Console.WriteLine($"{packetType} from {theirEndPoint}");

                var packet = new Packet(packetType);

                int offset = packetBlockOffset + individualPacketOffsets[i] + 1;
                int size = individualPacketSizes[i];

                packet.In(data, offset, size);

                totalPacketsReceived++;

                if (packetType == PacketType.LatencyCorrection) {
                    packet.Out(out Time theirLatency);


                    var latencyDifference = Math.Abs(theirLatency - PerceivedLatency);
                    var latencySign = Math.Sign(theirLatency - PerceivedLatency);

                    var offsetChange = (Time)Math.Round(Math.Min(Math.Pow(latencyDifference * 0.05d, 2d), latencyDifference * 0.5d) * latencySign);

                    //var offsetChange = (Time)Math.Round((PerceivedLatency - theirLatency) * 0.1d);
                    System.Console.WriteLine(offsetChange);
                    TimeOffset += offsetChange;

                    isLatencyCorrectionProfiling = true;

                    packet.ResetReadPosition();
                }

                PacketReceived?.Invoke(packet, theirEndPoint);
            }


        }

        if (Connected && Game.Time - lastSentTime >= sendRetryInterval) SendInternal();
    }

    private void SampleLatency(Time theirTime, Time theirPerceivedLatency) {
        var latency = Time - theirTime;




        DataReceived?.Invoke(latency);


        latencySamples.Enqueue(latency);

        correctionSamples.Enqueue(Time - theirTime);

        while (correctionSamples.Count > maxLatencyCorrectionSamples) correctionSamples.Dequeue();

        PerceivedLatency = GetAverageLatency();

        if (isLatencyCorrectionProfiling) {
            profilingCount++;

            if (profilingCount >= maxLatencyCorrectionSamples) {
                profilingCount = 0;
                isLatencyCorrectionProfiling = false;

                Send(new Packet(PacketType.LatencyCorrection).In(PerceivedLatency));
            }
        }

        // var offsetChange = (Time)Math.Round((PerceivedLatency - theirPerceivedLatency) * 0.01d);
        // System.Console.WriteLine(offsetChange);
        // TimeOffset -= offsetChange;






        // pingSamples.Enqueue((Time - theirTime) / 500000f);
        // while (pingSamples.Count > 100) {
        //     pingSamples.Dequeue();
        // }
    }

    private void DequeueOutgoingPackets(int totalTheyReceived) {
        int numToDequeue = packetOutgoingQueue.Count - (totalPacketsSent - totalTheyReceived);
        for (int i = 0; i < numToDequeue; i++) packetOutgoingQueue.Dequeue();
    }

    private void SendInternal() {

        var data = new byte[0]
        .Concat(BitConverter.GetBytes(Time))
        .Concat(BitConverter.GetBytes(PerceivedLatency))
        .Concat(BitConverter.GetBytes(totalPacketsSent))
        .Concat(BitConverter.GetBytes(totalPacketsReceived))
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

        udpClient.Send(data.ToArray());
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

    private Time GetAverageLatency() {
        double latencySum = 0d;
        foreach (var sample in correctionSamples) {
            latencySum += (double)sample;
        }

        return (long)Math.Round(latencySum / correctionSamples.Count);
    }

    public void StartLatencyCorrection() => isLatencyCorrectionProfiling = true;

    public void ResetPing() {
        correctionSamples.Clear();
    }
}