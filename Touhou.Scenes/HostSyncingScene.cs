
using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Generics;
using Touhou.Scenes;

namespace Touhou.Scenes;
public class HostSyncingScene : Scene {

    private readonly Text text;

    public HostSyncingScene() {

        text = new Text {
            DisplayedText = "Syncing...",
            CharacterSize = 40f,
            Origin = Vector2.UnitY,
            IsUI = true,
            Alignment = new Vector2(-1f, 1f),
        };
    }

    public override void OnInitialize() {
        Game.Network.Send(new Packet(PacketType.ConnectionResponse));

        AddEntity(new ReceiveCallback(ReceiveCallback));

        AddEntity(new RenderCallback(() => Game.Draw(text, Layer.UI1)));
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
                // packet.Out(out Time gameStartTime);
                // Game.Scenes.ChangeScene<MatchScene>(false, true, gameStartTime);
                // break;

                Game.Scenes.ChangeScene<CharacterSelectScene>(false, true);
                break;
        }
    }

    public override void OnDisconnect() {
        if (Game.Settings.UseSteam) Game.Network.DisconnectSteam();
        else Game.Network.Disconnect();

        Log.Warn("Opponent disconnected");

        Game.Scenes.ChangeScene<MainScene>();
    }

}
