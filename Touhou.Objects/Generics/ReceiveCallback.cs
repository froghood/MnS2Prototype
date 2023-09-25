using System.Net;
using Touhou.Networking;

namespace Touhou.Objects.Generics;

public class ReceiveCallback : Entity, IReceivable {

    private Action<Packet, IPEndPoint> callback;

    public ReceiveCallback(Action<Packet, IPEndPoint> callback) {
        this.callback = callback;
    }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        callback.Invoke(packet, endPoint);
    }
}