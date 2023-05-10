
using Touhou.Net;
using Touhou.Scenes.HostSyncing.Objects;

namespace Touhou.Scenes.HostSyncing;
public class HostSyncingScene : Scene {

    public override void OnInitialize() {
        Game.Network.Send(new Packet(PacketType.ConnectionResponse));

        var receiver = new Receiver();
        AddEntity(receiver);
    }
}
