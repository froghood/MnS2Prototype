using System.Net;
using System.Runtime.InteropServices;

namespace Touhou.Net;

public class Packet {

    public PacketType Type { get => type; }
    public byte[] Data { get => stream.ToArray(); }
    public int Size { get => (int)stream.Length; }

    private PacketType type;
    private MemoryStream stream;

    private long writePosition;
    private long readPosition;


    public Packet(PacketType type) {
        this.type = type;
        stream = new MemoryStream();
    }

    ~Packet() {
        stream.Dispose();
    }

    public Packet In(byte[] data, int offset, int count) {
        stream.Position = writePosition;
        stream.Write(data, offset, count);
        writePosition = stream.Position;
        return this;
    }

    public Packet In<T>(T data) where T : struct {
        var size = Marshal.SizeOf(data);
        var buffer = new byte[size];
        var ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(data, ptr, true);
        Marshal.Copy(ptr, buffer, 0, size);
        Marshal.FreeHGlobal(ptr);
        stream.Position = writePosition;
        stream.Write(buffer, 0, size);
        writePosition = stream.Position;
        return this;
    }

    public Packet Out<T>(out T data) where T : struct {
        var size = Marshal.SizeOf(typeof(T));
        var buffer = new byte[size];
        var ptr = Marshal.AllocHGlobal(size);
        stream.Position = readPosition;
        stream.Read(buffer, 0, size);
        readPosition = stream.Position;
        Marshal.Copy(buffer, 0, ptr, size);
        data = (T)Marshal.PtrToStructure(ptr, typeof(T));
        Marshal.FreeHGlobal(ptr);
        return this;
    }

    public void ResetReadPosition() => readPosition = 0;
    public void ResetWritePosition() => writePosition = 0;
}