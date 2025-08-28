
using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Generics;
using Touhou.Scenes;

namespace Touhou.Scenes;

public class HostSyncingTScene : TScene {

    private readonly Text text;

    public HostSyncingTScene() {

        text = new Text {
            DisplayedText = "Syncing...",
            CharacterSize = 40f,
            Origin = Vector2.UnitY,
            IsUI = true,
            Alignment = new Vector2(-1f, 1f),
        };
    }

    public override void OnInitialize() {
        Game.Get<Network>().Send(new Packet(PacketType.ConnectionResponse));

        AddEntity(new ReceiveCallback(ReceiveCallback));

        AddEntity(new RenderCallback(() => Game.Get<Renderer>().Queue(text, Layer.UI1)));
    }

    private void ReceiveCallback(Packet packet, IPEndPoint endPoint) {
        switch (packet.Type) {
            case PacketType.TimeRequest:
                //_connectionResponseFlag = true;
                packet.Out(out Time theirTime);
                var responsePacket = new Packet(PacketType.TimeResponse).In(theirTime).In(Game.Get<Network>().Time);
                //Console.WriteLine($"Received Time Request: {theirTime}");
                Game.Get<Network>().Send(responsePacket);
                break;

            case PacketType.SyncFinished:
                // packet.Out(out Time gameStartTime);
                // Game.Get<SceneManager>().ChangeTScene<MatchTScene>(false, true, gameStartTime);
                // break;

                Game.Get<SceneManager>().ChangeScene<CharacterSelecTScene>(false, true);
                break;
        }
    }

    public override void OnDisconnect() {
        if (Game.Get<Settings>().UseSteam) Game.Get<Network>().DisconnectSteam();
        else Game.Get<Network>().Disconnect();

        Log.Warn("Opponent disconnected");

        Game.Get<SceneManager>().ChangeScene<MainTScene>();
    }

}
