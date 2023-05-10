using System.Net;
using Newtonsoft.Json;
using SFML.Graphics;
using SFML.Window;
using Touhou.Scenes.Main.Objects;

namespace Touhou.Scenes.Main;

public class MainScene : Scene {

    private readonly Settings settings;

    public MainScene(Settings settings) {
        this.settings = settings;
    }

    public override void OnInitialize() {
        Game.ClearColor = new Color(20, 20, 25);

        var controller = new Controller(this.settings);
        AddEntity(controller);

    }
}
