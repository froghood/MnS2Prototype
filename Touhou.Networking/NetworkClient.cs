namespace Touhou.Networking;

public abstract class NetworkClient {

    public abstract bool IsConnected { get; }

    public abstract void Send(byte[] buffer);
    public abstract bool Receive(out byte[] data);
    public abstract void Close();
}