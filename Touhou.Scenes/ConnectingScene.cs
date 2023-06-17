using System.Net;
using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Scenes;
using Touhou.Objects.Generics;

namespace Touhou.Scenes;

public class ConnectingScene : Scene {

    private readonly Text text;

    private IPEndPoint endPoint;

    public ConnectingScene(IPEndPoint endPoint) {
        this.endPoint = endPoint;

        text = new Text("Connecting...", Game.DefaultFont, 14);
    }

    public override void OnInitialize() {
        Game.Network.TimeOffset -= Game.Time;

        Game.Window.SetTitle("MnS2 | Client");

        AddEntity(new Receiver((packet, endPoint) => {
            if (packet.Type != PacketType.ConnectionResponse) return;
            Game.Scenes.PushScene<ClientSyncingScene>();
        }));

        AddEntity(new Renderer(() => Game.Draw(text, 0)));

        TryConnecting(endPoint);

    }

    public override void OnDisconnect() {
        Game.Network.Disconnect();
        System.Console.WriteLine("disconnected");

        TryConnecting(endPoint);
    }

    private void TryConnecting(IPEndPoint endPoint) {
        Game.Network.Connect(endPoint);
        Game.Network.Send(new Packet(PacketType.Connection));

        System.Console.WriteLine($"Attempting to connect to {endPoint}");
    }
}