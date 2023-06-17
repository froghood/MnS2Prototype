
using SFML.Graphics;
using SFML.System;

using Touhou.Objects;
using Touhou.Objects.Characters;
using Touhou.Objects.Generics;

namespace Touhou.Scenes;

public class MatchScene : Scene {

    private readonly bool isHosting;
    private readonly Time startTime;

    private Action<Time> latencyGraphDelegate;
    private DistributionGraph latencyGraph;

    public MatchScene(bool isHosting, Time startTime) {

        this.isHosting = isHosting;
        this.startTime = startTime;

        latencyGraph = new DistributionGraph() { Size = new Vector2f(Game.Window.Size.X / 2f, 25) };
        latencyGraph.Position = new Vector2f(Game.Window.Size.X / 4f, 0f);
        latencyGraph.Add("latency", null, 100, Color.Green);

        latencyGraphDelegate = (latency) => latencyGraph.Sample("latency", (float)(latency / 1000d));

    }

    public override void OnInitialize() {

        var match = new Match(startTime);
        AddEntity(match);
        AddEntity(new Renderer(() => {
            var states = new RectangeStates() {
                Origin = new Vector2f(0.5f, 0.5f),
                OriginType = OriginType.Percentage,
                Size = match.Bounds * 2f,
                FillColor = Color.Transparent,
                OutlineColor = new Color(255, 255, 255, 60)
            };
            Game.DrawRectangle(states, 0);
        }));

        Game.Network.ResetPing();
        if (isHosting) Game.Network.StartLatencyCorrection();

        var opponent = new OpponentReimu(new Vector2f(!isHosting ? -200 : 200, 0f));
        var player = new PlayerReimu(isHosting) { Position = new Vector2f(isHosting ? -200 : 200, 0f) };
        //var player = new PlayerReimu() { Position = new Vector2f(80f, Game.Window.Size.Y / 2f) };

        AddEntity(player);
        AddEntity(opponent);

        AddEntity(new MatchUI(isHosting));

        AddEntity(new Renderer(() => {

            // var state = new RectangeStates() {
            //     Origin = new Vector2f(0.5f, 0.5f),
            //     Size = new Vector2f(768f, 768f),
            //     Position = new Vector2f(0.5f, 0.5f),
            //     Scale = new Vector2f(1f, 1f) * 0.25f,
            //     Rotation = Game.Time.AsSeconds() * 180f,
            //     IsUI = true,
            // };

            // var shader = new TShader("template");

            // Game.DrawRectangle(state, shader, Layers.UI2);





        }));

        // var graph = new Graph() { Size = new Vector2f(Game.Window.Size.X, 25) };
        // graph.Position = new Vector2f(0f, Game.Window.Size.Y - graph.Size.Y);
        // graph.Add("update", () => {
        //     Game.Debug.Fields.TryGet("update", out var value);
        //     return value;
        // }, 10000, Color.Yellow);
        // graph.Add("render", () => {
        //     Game.Debug.Fields.TryGet("render", out var value);
        //     return value;
        // }, 10000, Color.Blue);
        // graph.Add("step", () => {
        //     Game.Debug.Fields.TryGet("step", out var value);
        //     return value;
        // }, 10000, Color.Green);
        // AddEntity(graph);

        Game.Network.DataReceived += latencyGraphDelegate;

        //AddEntity(latencyGraph);

        AddEntity(new ValueDisplay<string>(() => $"FPS: {Game.FPS}") { Color = Color.White, CharacterSize = 14 });
        //AddEntity(new ValueDisplay<string>(() => $"Network Time: {Game.Network.Time.AsMilliseconds()}") { Position = new Vector2f(0f, 20f), Color = Color.White, CharacterSize = 14 });

        // AddEntity(new ValueDisplay<string>(() => $"Lat: {Game.Network.PerceivedLatency.AsMilliseconds()}") { Position = new Vector2f(0f, 50f), Color = Color.White, CharacterSize = 14 });
        //AddEntity(new ValueDisplay<string>(() => $"Their Lat: {Game.Network.TheirPerceivedLatency.AsMilliseconds()}") { Position = new Vector2f(0f, 70f), Color = Color.White, CharacterSize = 14 });

        //AddEntity(new ValueDisplay<string>(() => $"Pos: {player.Position.X}, {player.Position.Y}") { Position = new Vector2f(0f, 100f), Color = Color.White, CharacterSize = 14 });


    }

    // public override void OnRender() {


    //     if (_gameManager.StartTime - Game.Network.Time > 0 && _gameManager.StartTime - Game.Network.Time <= 3000) {
    //         var text = new Text($"{(_gameManager.StartTime - Game.Network.Time) / 1000f}", _font, 14);
    //         text.Position = new Vector2f(0, 60f);
    //         Game.Draw(text, 0);
    //     }

    //     _gameManager.OnRender();

    //     _fpsSamples.Enqueue(delta);

    //     while (_fpsSamples.Count > 1000) _fpsSamples.Dequeue();
    //     var fpsText = new Text($"FPS {MathF.Floor(1 / (_fpsSamples.Sum() / _fpsSamples.Count))}", _font, 14);
    //     Game.Draw(fpsText, 0);

    //     // var pingText = new Text($"Ping: [{Game.Network.Ping}]", _font, 14);
    //     // pingText.Position += new Vector2f(0, 20f);
    //     // Game.Draw(pingText, 0);

    //     // var frameTimesText = new Text($"Frames: [{Game.FrameTimes}]", _font, 14);

    //     // frameTimesText.Position += new Vector2f(0, 80f);
    //     // Game.Draw(frameTimesText, 0);

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