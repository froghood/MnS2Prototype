using System.Net;
using SFML.System;
using SFML.Graphics;
using SFML.Window;
using Touhou.Net;
using Touhou.Scenes.Hosting.Objects;

namespace Touhou.Scenes.Hosting;

public class HostingScene : Scene {

    private int port;

    public HostingScene(int port) {
        this.port = port;
    }

    public override void OnInitialize() {

        Game.Network.TimeOffset -= Game.Time;

        Game.Window.SetTitle("MnS2 | Host");
        Game.Network.Host(this.port);

        var receiver = new Receiver();
        AddEntity(receiver);
    }
}