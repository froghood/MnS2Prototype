using System.Net;
using Touhou.Networking;
using Touhou.Scenes;
using Touhou.Objects.Generics;
using Touhou.Graphics;
using OpenTK.Mathematics;

namespace Touhou.Scenes;

public class HostingScene : Scene {

    private readonly Text displayText;


    public HostingScene() {

        displayText = new Text {
            DisplayedText = "Waiting for connection...",
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

        Host();
    }


    private void Host() {
        if (Game.Settings.UseSteam) {

            Game.Network.WaitForConnection(HostSuccess, HostFailure);

        } else {

            Game.Network.WaitForConnection(
                Game.Settings.Port,
                HostSuccess,
                HostFailure);
        }
    }


    private void HostSuccess() {
        Game.Scenes.ChangeScene<CharacterSelectScene>(false, true);
    }


    private void HostFailure() {
        Game.Scenes.ChangeScene<MainScene>();
    }
}