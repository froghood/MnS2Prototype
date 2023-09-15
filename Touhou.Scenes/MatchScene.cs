

using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Objects;
using Touhou.Objects.Characters;
using Touhou.Objects.Generics;
using Touhou.Objects.Projectiles;

namespace Touhou.Scenes;

public class MatchScene : Scene {

    private readonly bool isHosting;
    private readonly Time startTime;

    private readonly Graph updateTimeGraph;
    private readonly Graph renderTimeGraph;

    private Action<Time> latencyGraphDelegate;

    public MatchScene(bool isHosting, Time startTime) {

        this.isHosting = isHosting;
        this.startTime = startTime;

        updateTimeGraph = new Graph(() => {
            Game.Stats.TryGet("update", out var value);
            return value;
        }, 5000) {
            Size = new Vector2(800f, 100f),
            Origin = Vector2.UnitX * 0.5f,
            Color = Color4.Green,
            IsUI = true,
            Alignment = new Vector2(0.42f, -1f),
        };

        renderTimeGraph = new Graph(() => {
            Game.Stats.TryGet("render", out var value);
            return value;
        }, 5000) {
            Size = new Vector2(800f, 100f),
            Origin = Vector2.UnitX * 0.5f,
            Color = Color4.LightBlue,
            IsUI = true,
            Alignment = new Vector2(0.42f, -1f),
        };

    }

    public override void OnInitialize() {

        Projectile.totalLocalProjectiles = 0;
        Projectile.totalRemoteProjectiles = 0x80000000;

        var match = new Match(startTime);
        AddEntity(match);
        AddEntity(new RenderCallback(() => {

            var matchBoundsRectangle = new Rectangle() {
                Origin = new Vector2(0.5f, 0.5f),
                Size = match.Bounds * 2f,
                FillColor = Color4.Transparent,
                StrokeColor = new Color4(255, 255, 255, 60),
                StrokeWidth = 1f,
            };

            Game.Draw(matchBoundsRectangle, Layers.Background2);




            Game.Draw(updateTimeGraph, Layers.UI1);
            Game.Draw(renderTimeGraph, Layers.UI1);

            int actionNumber = 0;
            foreach (var action in Game.Input.GetActionOrder()) {
                if (Game.Input.IsActionPressBuffered(action, out var time, out _)) {
                    var rect = new Rectangle() {
                        Origin = new Vector2(1f, 1f),
                        Size = new Vector2((300f - (float)(Game.Time - time).AsMilliseconds()) * 0.5f, 18f),
                        FillColor = Color4.White,
                        StrokeColor = Color4.Black,
                        StrokeWidth = 1f,
                        IsUI = true,
                        Alignment = new Vector2(0.99f, 0.99f - 0.05f * actionNumber),
                    };

                    Game.Draw(rect, Layers.UI1);

                    actionNumber++;
                }
            }


        }));

        Game.Network.ResetPing();
        if (isHosting) Game.Network.StartLatencyCorrection();

        var opponent = new OpponentReimu(new Vector2(!isHosting ? -200 : 200, 0f));
        var player = new PlayerReimu(isHosting) { Position = new Vector2(isHosting ? -200 : 200, 0f) };
        //var player = new PlayerReimu() { Position = new Vector2(80f, Game.Window.Size.Y / 2f) };

        AddEntity(player);
        AddEntity(opponent);

        AddEntity(new MatchUI(isHosting));


        Game.Network.DataReceived += latencyGraphDelegate;

        //AddEntity(latencyGraph);

        AddEntity(new ValueDisplay<string>(() => $"FPS: {Game.FPS}") {
            Origin = Vector2.UnitY,
            Color = Color4.White,
            CharacterSize = 40f,
            IsUI = true,
            UIAlignment = new Vector2(-1f, 1f),
        });

        AddEntity(new ValueDisplay<string>(() => $"Network Time: {Game.Network.Time.AsMilliseconds()}") {
            Origin = Vector2.UnitY,
            Position = new Vector2(0f, -70f),
            CharacterSize = 40f,
            Color = Color4.White,
            IsUI = true,
            UIAlignment = new Vector2(-1f, 1f),
        });

        AddEntity(new ValueDisplay<string>(() => $"Lat: {Game.Network.PerceivedLatency.AsMilliseconds()}") {
            Origin = Vector2.UnitY,
            Position = new Vector2(0f, -140f),
            CharacterSize = 40f,
            Color = Color4.White,
            IsUI = true,
            UIAlignment = new Vector2(-1f, 1f),
        });

        AddEntity(new ValueDisplay<string>(() => $"Their Lat: {Game.Network.TheirPerceivedLatency.AsMilliseconds()}") {
            Origin = Vector2.UnitY,
            Position = new Vector2(0f, -190f),
            CharacterSize = 40f,
            Color = Color4.White,
            IsUI = true,
            UIAlignment = new Vector2(-1f, 1f),
        });

        AddEntity(new ValueDisplay<string>(() => $"Pos: {player.Position.X}, {player.Position.Y}") {
            Origin = Vector2.UnitY,
            Position = new Vector2(0f, -260f),
            CharacterSize = 40f,
            Color = Color4.White,
            IsUI = true,
            UIAlignment = new Vector2(-1f, 1f),
        });
    }


    public override void OnDisconnect() {
        Game.Network.Disconnect();
        System.Console.WriteLine("disconnected");

        Game.Network.DataReceived -= latencyGraphDelegate;

        Game.Scenes.PopScene(); // -> HostSyncingScene / ClientSyncingScene
        Game.Scenes.PopScene(); // -> HostingScene / ConnectingScene
        Game.Scenes.PopScene(); // -> MainScene
    }
}