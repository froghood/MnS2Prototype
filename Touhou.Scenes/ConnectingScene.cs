using System.Net;
using Touhou.Networking;
using Touhou.Scenes;
using Touhou.Objects.Generics;
using Touhou.Graphics;
using OpenTK.Mathematics;
using Steamworks;

namespace Touhou.Scenes;

public class ConnectingScene : Scene {

    private readonly Text displayText;


    public ConnectingScene() {

        displayText = new Text {
            DisplayedText = "Connecting...",
            CharacterSize = 40f,
            Origin = Vector2.UnitY,
            IsUI = true,
            Alignment = new Vector2(-1f, 1f),
        };
    }


    public override void OnInitialize() {

        AddEntity(new RenderCallback(() => {
            Game.Draw(displayText, Layer.UI1);
        }));

        Connect();
    }


    public override void OnDisconnect() {
        Game.Scenes.ChangeScene<MainScene>();
    }


    private void Connect() {

        if (Game.Settings.UseSteam) {

            Game.Network.Connect(Game.Settings.SteamID);

        } else {

            Game.Network.Connect(new IPEndPoint(IPAddress.Parse(Game.Settings.Address), Game.Settings.Port));
        }

        Game.Network.Send(PacketType.Connection);
    }
}