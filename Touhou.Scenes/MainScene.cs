using System.Net;
using Newtonsoft.Json;
using Touhou.Scenes;
using Touhou.Objects.Generics;
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Scenes;

public class MainTScene : TScene {

    private readonly Text text;

    public MainTScene() {

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
                Game.Get<SceneManager>().ChangeScene<HostingTScene>();
            }

            if (action == PlayerActions.Secondary) {
                Game.Get<SceneManager>().ChangeScene<ConnectingTScene>();
            }
        }, (_) => { }));

        AddEntity(new RenderCallback(() => {

            Game.Get<Renderer>().Queue(text, Layer.UI1);

        }));

    }
}
