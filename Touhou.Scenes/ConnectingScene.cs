using System.Net;
using Touhou.Networking;
using Touhou.Scenes;
using Touhou.Objects.Generics;
using Touhou.Graphics;
using OpenTK.Mathematics;
using Steamworks;

namespace Touhou.Scenes;

public class ConnectingTScene : TScene {

    private readonly Text text;

    private IPEndPoint endPoint;

    public ConnectingTScene() {
        endPoint = new IPEndPoint(IPAddress.Parse(Game.Get<Settings>().Address), Game.Get<Settings>().Port);

        text = new Text {
            DisplayedText = "Connecting...",
            CharacterSize = 40f,
            Origin = Vector2.UnitY,
            IsUI = true,
            Alignment = new Vector2(-1f, 1f),
        };
    }

    public override void OnInitialize() {
        Game.Get<Network>().TimeOffset -= Game.Time;

        AddEntity(new UpdateCallback(OnUpdate));


        AddEntity(new ReceiveCallback((packet, endPoint) => {
            if (packet.Type != PacketType.ConnectionResponse) return;
            Game.Get<SceneManager>().ChangeScene<ClientSyncingTScene>();
        }));

        AddEntity(new RenderCallback(() => {
            Game.Get<Renderer>().Queue(text, Layer.UI1);
        }));
    }

    private void OnUpdate() {
        if (!Game.Get<Network>().IsConnected) {
            Connect();
        }
    }

    public override void OnDisconnect() {
        if (Game.Get<Settings>().UseSteam) Game.Get<Network>().DisconnectSteam();
        else Game.Get<Network>().Disconnect();
    }

    private void Connect() {
        if (Game.Get<Settings>().UseSteam) Game.Get<Network>().ConnectSteam(Game.Get<Settings>().SteamID);
        else Game.Get<Network>().Connect(endPoint);



        Game.Get<Network>().Send(new Packet(PacketType.Connection));

        Log.Info($"Attempting to connect to {Game.Get<Settings>().SteamID}");
    }
}