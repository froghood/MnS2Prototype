using System.Net;

namespace Touhou.Networking;

public class Packet {




    public Packet(PacketType type, byte[] data) {
        this.Type = type;
        this.data = data;
    }

    public PacketType Type { get; }

    private byte[] data;

    private int readPosition;


    public unsafe Packet Out<T>(out T value, bool fromStart = false) where T : unmanaged {

        fixed (byte* dataPtr = data) {

            // cast pointer to pointer of generic type starting at the read position
            T* ptr = (T*)(dataPtr + (fromStart ? 0 : readPosition));

            value = *ptr;
        }

        readPosition += sizeof(T);

        return this;
    }

    public unsafe Packet Out(out string str, bool fromStart = false) {
        str = "";

        return this;
    }

    public void ResetReadPosition() => readPosition = 0;
}