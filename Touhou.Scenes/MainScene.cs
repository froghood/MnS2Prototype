using System.Net;
using Newtonsoft.Json;
using Touhou.Scenes;
using Touhou.Objects.Generics;
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Scenes;

public class MainScene : Scene {

    private readonly Text text;

    public MainScene() {

        text = new Text {
            DisplayedText = $"Press {PlayerActions.Primary} to host, {PlayerActions.Secondary} to connect",
            Font = "consolas",
            CharacterSize = 40f,
            Origin = Vector2.UnitY * 1f,
            Color = Color4.White,
            Boldness = 0f,
            IsUI = true,
            Alignment = new Vector2(-1f, 1f),
        };

    }

    public override void OnInitialize() {
        AddEntity(new Controller((action) => {
            if (action == PlayerActions.Primary) {
                Game.Scenes.ChangeScene<HostingScene>();
            }

            if (action == PlayerActions.Secondary) {
                Game.Scenes.ChangeScene<ConnectingScene>();
            }
        }, (_) => { }));

        AddEntity(new RenderCallback(() => {

            Game.Draw(text, Layers.UI1);

        }));

    }
}
