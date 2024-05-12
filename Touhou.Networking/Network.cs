using System.Net;
using System.Runtime.InteropServices;
using Steamworks;

namespace Touhou.Networking;

public class Network {

    public event Action<Packet>? PacketReceived;

    public Time Time => Game.Time + timeOffset;

    private NetworkClient? networkClient;

    private bool isConnected;

    private int totalUniquePacketsSent;
    private int totalUniquePacketsReceived;

    private Time timeOffset;

    private Queue<LatencySample> latencySamples = new();



    private Queue<byte[]> sendingBuffer = new();
    private Queue<byte[]> sendingQueue = new();

    private Time lastSentTime;
    private Time autoSendInterval = Time.InMilliseconds(100);

    public void Connect(IPEndPoint endPoint) {

        networkClient?.Close();

        var client = new DirectNetworkClient();
        client.Connect(endPoint);

        networkClient = client;
    }


    public void Connect(SteamId id) {

        networkClient?.Close();

        var client = new SteamNetworkClient();
        client.Connect(id);

        networkClient = client;
    }


    public void Host(int port) {

        networkClient?.Close();

        var client = new DirectNetworkClient();
        client.Host(port);

        networkClient = client;
    }


    public unsafe void Send(PacketType type) {

        var data = new byte[sizeof(int) + sizeof(PacketType)];

        fixed (byte* dataPtr = data) {

            PacketType* typePtr = (PacketType*)(dataPtr + sizeof(int));
            *typePtr = type;
        }

        sendingBuffer.Enqueue(data);
        totalUniquePacketsSent++;
    }


    public unsafe void Send(PacketType type, params object[] values) {

        if (!isConnected) return;

        var sizes = new int[values.Length];
        int size = 0;

        Log.Info($"Sending {type}, values: {values.Length}");

        for (int i = 0; i < values.Length; i++) {

            var value = values[i];
            int valueSize = Marshal.SizeOf(value);

            sizes[i] = valueSize;
            size += valueSize;

            Log.Info($"{value.GetType}: {valueSize}");
        }

        var data = new byte[sizeof(int) + sizeof(PacketType) + size];
        fixed (byte* dataPtr = data) {

            nint offset = (nint)dataPtr;

            Marshal.StructureToPtr(size, offset, false);
            offset += sizeof(int);

            Marshal.StructureToPtr(type, offset, false);
            offset += sizeof(PacketType);

            for (int i = 0; i < values.Length; i++) {

                var value = values[i];
                var valueSize = sizes[i];

                Marshal.StructureToPtr(value, offset, false);
                offset += valueSize;
            }
        }

        sendingBuffer.Enqueue(data);
        totalUniquePacketsSent++;
    }




    public unsafe void Update() {

        while (networkClient?.Receive(out var source) ?? false) {

            fixed (byte* sourcePtr = source) {

                int offset = 0;

                var theirTime = Deserialize<Time>(sourcePtr, ref offset);
                var theirPerceivedLatency = Deserialize<Time>(sourcePtr, ref offset);
                var totalTheySent = Deserialize<int>(sourcePtr, ref offset);
                var totalTheyReceived = Deserialize<int>(sourcePtr, ref offset);
                var numPackets = Deserialize<int>(sourcePtr, ref offset);

                int numToRead = totalTheySent - totalUniquePacketsReceived;

                for (int i = numToRead - numPackets; i < numToRead; i++) {

                    var size = Deserialize<int>(sourcePtr, ref offset);
                    var packetType = Deserialize<PacketType>(sourcePtr, ref offset);

                    var packetData = new byte[size];
                    System.Buffer.BlockCopy(source, offset, packetData, 0, size);

                    offset += size;

                    Log.Info($"size: {size}, type: {packetType}, data: {string.Join(", ", packetData)}");

                    var packet = new Packet(packetType, packetData);

                    totalUniquePacketsReceived++;

                    PacketReceived?.Invoke(packet);
                }
            }
        }

        T Deserialize<T>(byte* ptr, ref int offset) where T : unmanaged {
            var value = (T*)(ptr + offset);
            offset += sizeof(T);
            return *value;
        }
    }

    public void Flush() {

        if (!networkClient?.IsConnected ?? true) return;

        if (sendingBuffer.Count > 0) {
            while (sendingBuffer.Count > 0) {

                sendingQueue.Enqueue(sendingBuffer.Dequeue());
            }

            this.SendInternal();

        } else {

            if (Game.Time - lastSentTime >= autoSendInterval) this.SendInternal();
        }



    }

    private unsafe void SendInternal() {

        var size =
            sizeof(Time) +
            sizeof(Time) +
            sizeof(int) +
            sizeof(int) +
            sizeof(int) +
            sendingQueue.Sum(e => e.Length);

        var data = new byte[size];
        int offset = 0;

        Serialize(data, ref offset, Time);
        Serialize(data, ref offset, GetPerceivedLatency());
        Serialize(data, ref offset, totalUniquePacketsSent);
        Serialize(data, ref offset, totalUniquePacketsReceived);
        Serialize(data, ref offset, sendingQueue.Count);

        while (sendingQueue.Count > 0) {
            var message = sendingQueue.Dequeue();
            System.Buffer.BlockCopy(message, 0, data, offset, message.Length);
        }

        networkClient?.Send(data);

        lastSentTime = Game.Time;

        void Serialize<T>(byte[] dst, ref int offset, T src) where T : unmanaged {
            fixed (byte* dstPtr = dst) {
                *(T*)(dstPtr + offset) = src;
                offset += sizeof(T);
            }
        }
    }



    private void SampleLatency(Time theirTime) {

        while (latencySamples.Count >= 100) DequeueSample();

        var latency = this.Time - theirTime;

        var sample = new LatencySample(latency.AsSeconds());


        foreach (var other in latencySamples) {
            var weight = 1f / (MathF.Abs(sample.Latency - other.Latency) + 1f);
            sample.Weigh(weight);
            other.Weigh(weight);
        }

        latencySamples.Enqueue(sample);

    }

    private void DequeueSample() {
        var sample = latencySamples.Dequeue();
        foreach (var other in latencySamples) {
            var weight = 1f / (MathF.Abs(sample.Latency - other.Latency) + 1f);
            other.Weigh(-weight);
        }
    }

    private Time GetPerceivedLatency() {

        float maxWeight = latencySamples.Max(e => e.Weight);
        float minWeight = latencySamples.Min(e => e.Weight);

        float totalLatency = 0f;
        int count = 0;

        foreach (var sample in latencySamples) {
            if (sample.Weight < (maxWeight + minWeight) / 2f) continue;
            totalLatency += sample.Latency;
            count++;
        }

        return Time.InSeconds(totalLatency / count);
    }
}