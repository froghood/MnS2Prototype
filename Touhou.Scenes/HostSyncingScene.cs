
using System.Net;
using SFML.Graphics;
using Touhou.Net;
using Touhou.Objects.Generics;
using Touhou.Scenes;

namespace Touhou.Scenes;
public class HostSyncingScene : Scene {

    private readonly Text text;

    public HostSyncingScene() {
        text = new Text("Syncing...", Game.DefaultFont, 14);
    }

    public override void OnInitialize() {
        Game.Network.Send(new Packet(PacketType.ConnectionResponse));

        AddEntity(new Receiver(ReceiveCallback));
        AddEntity(new Renderer(() => Game.Draw(text, 0)));
    }

    private void ReceiveCallback(Packet packet, IPEndPoint endPoint) {
        switch (packet.Type) {
            case PacketType.TimeRequest:
                //_connectionResponseFlag = true;
                packet.Out(out Time theirTime);
                var responsePacket = new Packet(PacketType.TimeResponse).In(theirTime).In(Game.Network.Time);
                //Console.WriteLine($"Received Time Request: {theirTime}");
                Game.Network.Send(responsePacket);
                break;

            case PacketType.SyncFinished:
                packet.Out(out Time gameStartTime);
                Game.Scenes.PushScene<MatchScene>(true, gameStartTime);
                break;
        }
    }

}
