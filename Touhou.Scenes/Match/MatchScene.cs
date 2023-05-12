using System.Collections;
using System.Net;
using System.Linq;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Scenes.Match.Objects;
using Touhou.Scenes.Match.Objects.Characters;

namespace Touhou.Scenes.Match;

public class MatchScene : Scene {

    //private Queue<float> _fpsSamples = new();

    //private Clock _retryTimer = new();

    private readonly bool hosting;
    private readonly Time startTime;

    private Action<Time> latencyGraphDelegate;
    private DistributionGraph latencyGraph;

    public MatchScene(bool hosting, Time startTime) {

        this.hosting = hosting;
        this.startTime = startTime;

        latencyGraph = new DistributionGraph() { Size = new Vector2f(Game.Window.Size.X / 2f, 25) };
        latencyGraph.Position = new Vector2f(Game.Window.Size.X / 4f, 0f);
        latencyGraph.Add("latency", null, 100, Color.Green);

        latencyGraphDelegate = (latency) => latencyGraph.Sample("latency", (float)(latency / 1000d));

    }

    public override void OnInitialize() {

        AddEntity(new MatchTimer(startTime));

        Game.Network.ResetPing();
        if (hosting) Game.Network.StartLatencyCorrection();

        var player = new PlayerReimu() { Position = new Vector2f(Game.Window.Size.X / (hosting ? 3f : 1.5f), Game.Window.Size.Y / 2f) };
        //var player = new PlayerReimu() { Position = new Vector2f(80f, Game.Window.Size.Y / 2f) };
        var opponent = new OpponentReimu(new Vector2f(Game.Window.Size.X / (!hosting ? 3f : 1.5f), Game.Window.Size.Y / 2f));

        AddEntity(player);
        AddEntity(opponent);

        var graph = new Graph() { Size = new Vector2f(Game.Window.Size.X, 25) };
        graph.Position = new Vector2f(0f, Game.Window.Size.Y - graph.Size.Y);
        graph.Add("update", () => {
            Game.Debug.Fields.TryGet("update", out var value);
            return value;
        }, 10000, Color.Yellow);
        graph.Add("render", () => {
            Game.Debug.Fields.TryGet("render", out var value);
            return value;
        }, 10000, Color.Blue);
        graph.Add("step", () => {
            Game.Debug.Fields.TryGet("step", out var value);
            return value;
        }, 10000, Color.Green);
        AddEntity(graph);

        Game.Network.DataReceived += latencyGraphDelegate;

        AddEntity(latencyGraph);

        AddEntity(new ValueDisplay<float>(() => Game.FPS) { Color = Color.White, CharacterSize = 14 });
        AddEntity(new ValueDisplay<Time>(() => Game.Network.Time) { Position = new Vector2f(0f, 20f), Color = Color.White, CharacterSize = 14 });

        AddEntity(new ValueDisplay<Time>(() => Game.Network.PerceivedLatency) { Position = new Vector2f(0f, 50f), Color = Color.White, CharacterSize = 14 });
        AddEntity(new ValueDisplay<Time>(() => Game.Network.TheirPerceivedLatency) { Position = new Vector2f(0f, 70f), Color = Color.White, CharacterSize = 14 });

        AddEntity(new ValueDisplay<Vector2f>(() => player.Position) { Position = new Vector2f(0f, 100f), Color = Color.White, CharacterSize = 14 });


    }

    // public override void OnRender() {


    //     if (_gameManager.StartTime - Game.Network.Time > 0 && _gameManager.StartTime - Game.Network.Time <= 3000) {
    //         var text = new Text($"{(_gameManager.StartTime - Game.Network.Time) / 1000f}", _font, 14);
    //         text.Position = new Vector2f(0, 60f);
    //         Game.Window.Draw(text);
    //     }

    //     _gameManager.OnRender();

    //     _fpsSamples.Enqueue(delta);

    //     while (_fpsSamples.Count > 1000) _fpsSamples.Dequeue();
    //     var fpsText = new Text($"FPS {MathF.Floor(1 / (_fpsSamples.Sum() / _fpsSamples.Count))}", _font, 14);
    //     Game.Window.Draw(fpsText);

    //     // var pingText = new Text($"Ping: [{Game.Network.Ping}]", _font, 14);
    //     // pingText.Position += new Vector2f(0, 20f);
    //     // Game.Window.Draw(pingText);

    //     // var frameTimesText = new Text($"Frames: [{Game.FrameTimes}]", _font, 14);

    //     // frameTimesText.Position += new Vector2f(0, 80f);
    //     // Game.Window.Draw(frameTimesText);

    // }

    public override void OnDisconnect() {
        Game.Network.Disconnect();
        System.Console.WriteLine("disconnected");

        Game.Network.DataReceived -= latencyGraphDelegate;

        Game.Scenes.PopScene(); // -> HostSyncingScene / ClientSyncingScene
        Game.Scenes.PopScene(); // -> HostingScene / ConnectingScene
        Game.Scenes.PopScene(); // -> MainScene
    }
}