using System.Net;
using SFML.Graphics;
using Touhou.Objects;

using Touhou.Scenes.Hosting;
using Touhou.Scenes.Connecting;

namespace Touhou.Scenes.Main.Objects;

public class Controller : Entity, IControllable {
    private readonly Text text;

    public Controller() {
        this.text = new Text($"Press {PlayerAction.Primary} to host, {PlayerAction.Secondary} to connect", Game.DefaultFont, 14);
    }

    public void Press(PlayerAction action) {
        if (action == PlayerAction.Primary) {
            Game.Scenes.PushScene<HostingScene>(Game.Settings.Port);
        }

        if (action == PlayerAction.Secondary) {
            Game.Scenes.PushScene<ConnectingScene>(new IPEndPoint(IPAddress.Parse(Game.Settings.Address), Game.Settings.Port));
        }
    }


    public void Release(PlayerAction action) { }

    public override void Update() { }

    public override void Render() {
        Game.Window.Draw(this.text);
    }

    public override void PostRender() { }
}