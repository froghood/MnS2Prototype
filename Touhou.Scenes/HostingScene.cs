using System.Net;
using SFML.System;
using SFML.Graphics;
using SFML.Window;
using Touhou.Net;
using Touhou.Scenes;
using Touhou.Objects.Generics;

namespace Touhou.Scenes;

public class HostingScene : Scene {

    private readonly Text text;

    private int port;

    public HostingScene(int port) {
        this.port = port;

        text = new Text("Waiting for connection...", Game.DefaultFont, 14);
    }

    public override void OnInitialize() {

        Game.Network.TimeOffset -= Game.Time;

        Game.Window.SetTitle("MnS2 | Host");
        Game.Network.Host(this.port);

        AddEntity(new Receiver(ReceiverCallback));
        AddEntity(new Renderer(() => Game.Draw(text, 0)));
    }

    private void ReceiverCallback(Packet packet, IPEndPoint endPoint) {
        if (packet.Type != PacketType.Connection || Game.Network.Connected) return;
        Game.Network.Connect(endPoint);
        Game.Scenes.PushScene<HostSyncingScene>();
    }
}