using System.Net;
using Touhou.Networking;
using Touhou.Scenes;
using Touhou.Objects.Generics;
using Touhou.Graphics;
using OpenTK.Mathematics;

namespace Touhou.Scenes;

public class HostingScene : Scene {

    private readonly Text text;

    private int port;

    public HostingScene() {
        this.port = Game.Settings.Port;

        text = new Text {
            DisplayedText = "Waiting for connection...",
            CharacterSize = 40f,
            Origin = Vector2.UnitY,
            IsUI = true,
            Alignment = new Vector2(-1f, 1f),
        };
    }

    public override void OnInitialize() {

        Game.Network.TimeOffset -= Game.Time;

        if (Game.Settings.UseSteam) Game.Network.HostSteam();
        else Game.Network.Host(port);

        AddEntity(new ReceiveCallback(ReceiverCallback));

        AddEntity(new RenderCallback(() => {
            Game.Draw(text, Layers.UI1);
        }));
    }

    private void ReceiverCallback(Packet packet, IPEndPoint endPoint) {
        if (packet.Type != PacketType.Connection) return;

        if (!Game.Settings.UseSteam) Game.Network.Connect(endPoint);

        Game.Scenes.ChangeScene<HostSyncingScene>();
    }
}