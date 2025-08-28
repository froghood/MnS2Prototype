using System.Net;
using Touhou.Networking;
using Touhou.Scenes;
using Touhou.Objects.Generics;
using Touhou.Graphics;
using OpenTK.Mathematics;

namespace Touhou.Scenes;

public class HostingTScene : TScene {

    private readonly Text text;

    private int port;

    public HostingTScene() {
        this.port = Game.Get<Settings>().Port;

        text = new Text {
            DisplayedText = "Waiting for connection...",
            CharacterSize = 40f,
            Origin = Vector2.UnitY,
            IsUI = true,
            Alignment = new Vector2(-1f, 1f),
        };
    }

    public override void OnInitialize() {

        Game.Get<Network>().TimeOffset -= Game.Time;

        if (Game.Get<Settings>().UseSteam) Game.Get<Network>().HostSteam();
        else Game.Get<Network>().Host(port);

        AddEntity(new ReceiveCallback(ReceiverCallback));

        AddEntity(new RenderCallback(() => {
            Game.Get<Renderer>().Queue(text, Layer.UI1);
        }));
    }

    private void ReceiverCallback(Packet packet, IPEndPoint endPoint) {
        if (packet.Type != PacketType.Connection) return;

        if (!Game.Get<Settings>().UseSteam) Game.Get<Network>().Connect(endPoint);

        Game.Get<SceneManager>().ChangeScene<HostSyncingTScene>();
    }
}