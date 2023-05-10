using System.Net;
using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Scenes.Connecting.Objects;

namespace Touhou.Scenes.Connecting;

public class ConnectingScene : Scene {

    private IPEndPoint endPoint;

    public ConnectingScene(IPEndPoint endPoint) {
        this.endPoint = endPoint;
    }

    public override void OnInitialize() {
        Game.Network.TimeOffset -= Game.Time;

        Game.Window.SetTitle("MnS2 | Client");

        var receiver = new Receiver();
        AddEntity(receiver);

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