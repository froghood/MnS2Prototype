using System.Net;
using Touhou.Networking;
using Touhou.Scenes;
using Touhou.Objects.Generics;
using Touhou.Graphics;
using OpenTK.Mathematics;
using Steamworks;

namespace Touhou.Scenes;

public class ConnectingScene : Scene {

    private readonly Text text;

    private IPEndPoint endPoint;

    public ConnectingScene() {
        endPoint = new IPEndPoint(IPAddress.Parse(Game.Settings.Address), Game.Settings.Port);

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

        AddEntity(new UpdateCallback(OnUpdate));


        AddEntity(new ReceiveCallback((packet, endPoint) => {
            if (packet.Type != PacketType.ConnectionResponse) return;
            Game.Scenes.PushScene<ClientSyncingScene>();
        }));

        AddEntity(new RenderCallback(() => {
            Game.Draw(text, Layers.UI1);
        }));
    }

    private void OnUpdate() {
        if (!Game.Network.IsConnected) {
            Connect();
        }
    }

    public override void OnDisconnect() {
        if (Game.Settings.UseSteam) Game.Network.DisconnectSteam();
        else Game.Network.Disconnect();
    }

    private void Connect() {
        if (Game.Settings.UseSteam) Game.Network.ConnectSteam(Game.Settings.SteamID);
        else Game.Network.Connect(endPoint);



        Game.Network.Send(new Packet(PacketType.Connection));

        System.Console.WriteLine($"Attempting to connect to {Game.Settings.SteamID}");
    }
}