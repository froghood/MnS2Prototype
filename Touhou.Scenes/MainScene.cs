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
        //text = new Text($"Press {PlayerAction.Primary} to host, {PlayerAction.Secondary} to connect", Game.DefaultFont, 14);

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
        //Game.Renderer.ClearColor4 = new Color44(1f, 1f, 1f, 1f);

        AddEntity(new Controller((action) => {
            if (action == PlayerActions.Primary) {
                Game.Scenes.PushScene<HostingScene>(Game.Settings.Port);
            }

            if (action == PlayerActions.Secondary) {
                Game.Scenes.PushScene<ConnectingScene>(new IPEndPoint(IPAddress.Parse(Game.Settings.Address), Game.Settings.Port));
            }
        }, (_) => { }));

        AddEntity(new RenderCallback(() => {

            var circle = new Circle() {
                Origin = new Vector2(0.5f),
                Radius = 100f,
                IsUI = true,
            };

            Game.Draw(circle, Layers.UI1);

            Game.Draw(text, Layers.UI1);
            //text.CharacterSize += 50f * Game.Delta.AsSeconds();
            //text.Boldness = MathF.Sin(Game.Delta.AsSeconds());

            //System.Console.WriteLine(Game.WindowSize);
        }));

    }
}
