using Touhou.Objects;

namespace Touhou.Networking;

public class NetworkSubscriber {
    public Entity User { get; }
    public ReceiveCallback ReceiveCallback { get; }

    public NetworkSubscriber(Entity user, ReceiveCallback receiveCallback) {
        User = user;
        ReceiveCallback = receiveCallback;
    }
}

public delegate void ReceiveCallback(Packet packet);