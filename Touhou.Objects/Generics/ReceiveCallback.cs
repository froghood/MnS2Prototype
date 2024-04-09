using System.Net;
using Touhou.Networking;

namespace Touhou.Objects.Generics;

public class ReceiveCallback : Entity, IReceivable {

    private Action<Packet> callback;

    public ReceiveCallback(Action<Packet> callback) {
        this.callback = callback;
    }

    public void Receive(Packet packet) {
        callback.Invoke(packet);
    }
}