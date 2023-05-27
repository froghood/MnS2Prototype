using System.Net;
using Newtonsoft.Json;
using SFML.Graphics;
using SFML.Window;
using Touhou.Scenes;
using Touhou.Objects.Generics;

namespace Touhou.Scenes;

public class MainScene : Scene {

    private readonly Text text;

    public MainScene() {
        text = new Text($"Press {PlayerAction.Primary} to host, {PlayerAction.Secondary} to connect", Game.DefaultFont, 14);
    }

    public override void OnInitialize() {
        Game.ClearColor = new Color(20, 20, 25);

        AddEntity(new Controller((action) => {
            if (action == PlayerAction.Primary) {
                Game.Scenes.PushScene<HostingScene>(Game.Settings.Port);
            }

            if (action == PlayerAction.Secondary) {
                Game.Scenes.PushScene<ConnectingScene>(new IPEndPoint(IPAddress.Parse(Game.Settings.Address), Game.Settings.Port));
            }
        }, (_) => { }));

        AddEntity(new Renderer(() => Game.Window.Draw(text)));

    }
}
