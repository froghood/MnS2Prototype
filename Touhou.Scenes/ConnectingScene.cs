using System.Net;
using Touhou.Net;
using Touhou.Scenes;
using Touhou.Objects.Generics;
using Touhou.Graphics;
using OpenTK.Mathematics;

namespace Touhou.Scenes;

public class ConnectingScene : Scene {

    private readonly Text text;

    private IPEndPoint endPoint;

    public ConnectingScene(IPEndPoint endPoint) {
        this.endPoint = endPoint;

        text = new Text {
            DisplayedText = "Connecting...",
            CharacterSize = 40f,
            Origin = Vector2.UnitY,
            IsUI = true,
            Alignment = new Vector2(-1f, 1f),
        };
    }

    public override void OnInitialize() {
        Game.Network.TimeOffset -= Game.Time;

        AddEntity(new ReceiveCallback((packet, endPoint) => {
            if (packet.Type != PacketType.ConnectionResponse) return;
            Game.Scenes.PushScene<ClientSyncingScene>();
        }));

        AddEntity(new RenderCallback(() => {
            Game.Draw(text, Layers.UI1);
        }));

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