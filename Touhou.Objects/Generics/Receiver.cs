using System.Net;
using Touhou.Net;

namespace Touhou.Objects.Generics;

public class Receiver : Entity, IReceivable {

    private Action<Packet, IPEndPoint> callback;

    public Receiver(Action<Packet, IPEndPoint> callback) {
        this.callback = callback;
    }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        callback.Invoke(packet, endPoint);
    }
}