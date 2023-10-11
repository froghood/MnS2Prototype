

using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Objects;
using Touhou.Objects.Characters;
using Touhou.Objects.Generics;
using Touhou.Objects.Projectiles;

namespace Touhou.Scenes;

public class MatchScene : Scene {

    private readonly bool isP1;
    private readonly Time startTime;
    private readonly Type playerType;
    private readonly Type opponentType;
    private readonly Graph updateTimeGraph;
    private readonly Graph renderTimeGraph;
    private readonly Player player;
    private readonly Opponent opponent;

    //private Action<Time> latencyGraphDelegate;

    public MatchScene(bool isP1, Time startTime, Type playerType, Type opponentType) {

        this.isP1 = isP1;
        this.startTime = startTime;
        this.playerType = playerType;
        this.opponentType = opponentType;

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

        player = (Player)Activator.CreateInstance(playerType, new object[] { isP1 });
        opponent = (Opponent)Activator.CreateInstance(opponentType, new object[] { isP1 });
    }

    public override void OnInitialize() {

        Projectile.TotalLocalProjectiles = 0;
        Projectile.TotalRemoteProjectiles = 0x80000000;

        var match = new Match(isP1, startTime, player, opponent);
        AddEntity(match);

        AddEntity(player);
        AddEntity(opponent);


        AddEntity(new RenderCallback(() => {

            var matchBoundsRectangle = new Rectangle() {
                Origin = new Vector2(0.5f, 0.5f),
                Size = match.Bounds * 2f,
                FillColor = Color4.Transparent,
                StrokeColor = new Color4(255, 255, 255, 60),
                StrokeWidth = 1f,
            };

            Game.Draw(matchBoundsRectangle, Layer.Background2);




            // Game.Draw(updateTimeGraph, Layers.UI1);
            // Game.Draw(renderTimeGraph, Layers.UI1);

            // int actionNumber = 0;
            // foreach (var action in Game.Input.GetActionOrder()) {
            //     if (Game.Input.IsActionPressBuffered(action, out var time, out _)) {
            //         var rect = new Rectangle() {
            //             Origin = new Vector2(1f, 1f),
            //             Size = new Vector2((300f - (float)(Game.Time - time).AsMilliseconds()) * 0.5f, 18f),
            //             FillColor = Color4.White,
            //             StrokeColor = Color4.Black,
            //             StrokeWidth = 1f,
            //             IsUI = true,
            //             Alignment = new Vector2(0.99f, 0.99f - 0.05f * actionNumber),
            //         };

            //         Game.Draw(rect, Layers.UI1);

            //         actionNumber++;
            //     }
            // }

            // var localProjectileHistroyDisplay = new ProjectileHistoryDisplay("local", Projectile.LocalProjectileHistory) {
            //     Origin = new Vector2(0f, 1f),
            //     Size = new Vector2(2600f, 80f),
            //     FillColor = new Color4(0f, 0f, 0f, 0.5f),
            //     StrokeColor = Color4.White,
            //     StrokeWidth = 1f,
            //     IsUI = true,
            //     Alignment = new Vector2(-0.9f, 0.99f),
            // };
            // Game.Draw(localProjectileHistroyDisplay, Layers.UI1);

            // var remoteProjectileHistroyDisplay = new ProjectileHistoryDisplay("remote", Projectile.RemoteProjectileHistory) {
            //     Origin = new Vector2(0f, 1f),
            //     Size = new Vector2(2600f, 80f),
            //     FillColor = new Color4(0f, 0f, 0f, 0.5f),
            //     StrokeColor = Color4.White,
            //     StrokeWidth = 1f,
            //     IsUI = true,
            //     Alignment = new Vector2(-0.9f, 0.905f),
            // };
            // Game.Draw(remoteProjectileHistroyDisplay, Layers.UI1);


        }));

        Game.Network.ResetPing();
        if (isP1) Game.Network.StartLatencyCorrection();

        //var player = new PlayerReimu() { Position = new Vector2(80f, Game.Window.Size.Y / 2f) };

        //AddEntity(player);
        //AddEntity(opponent);

        AddEntity(new UpdateCallback(() => {

            var distance = MathF.Sqrt(
                MathF.Pow(opponent.Position.X - player.Position.X, 2f) +
                MathF.Pow(opponent.Position.Y - player.Position.Y, 2f)
            );

            float zoom = MathF.Max(MathF.Min((distance - 250f) / 750f, 1f), 0f);

            var targetView = new Vector2(1600f, 900f) * (0.9f + 0.1f * zoom);
            Game.Camera.View += (targetView - Game.Camera.View) * (1f - MathF.Pow(0.05f, Game.Delta.AsSeconds()));

            var targetPosition = (player.Position + opponent.Position) / new Vector2(3f, 9f);
            Game.Camera.Position += (targetPosition - Game.Camera.Position) * (1f - MathF.Pow(0.05f, Game.Delta.AsSeconds()));
        }));

        AddEntity(new MatchUI(isP1));


        //Game.Network.DataReceived += latencyGraphDelegate;

        //AddEntity(latencyGraph);

        // AddEntity(new ValueDisplay<string>(() => $"FPS: {Game.FPS}") {
        //     Origin = Vector2.UnitY,
        //     Color = Color4.White,
        //     CharacterSize = 40f,
        //     IsUI = true,
        //     UIAlignment = new Vector2(0.75f, 1f),
        // });

        // AddEntity(new ValueDisplay<string>(() => $"Network Time: {Game.Network.Time.AsMilliseconds()}") {
        //     Origin = Vector2.UnitY,
        //     Position = new Vector2(0f, -70f),
        //     CharacterSize = 40f,
        //     Color = Color4.White,
        //     IsUI = true,
        //     UIAlignment = new Vector2(-1f, 1f),
        // });

        // AddEntity(new ValueDisplay<string>(() => $"Lat: {Game.Network.PerceivedLatency.AsMilliseconds()}") {
        //     Origin = Vector2.UnitY,
        //     Position = new Vector2(0f, -140f),
        //     CharacterSize = 40f,
        //     Color = Color4.White,
        //     IsUI = true,
        //     UIAlignment = new Vector2(-1f, 1f),
        // });

        // AddEntity(new ValueDisplay<string>(() => $"Their Lat: {Game.Network.TheirPerceivedLatency.AsMilliseconds()}") {
        //     Origin = Vector2.UnitY,
        //     Position = new Vector2(0f, -190f),
        //     CharacterSize = 40f,
        //     Color = Color4.White,
        //     IsUI = true,
        //     UIAlignment = new Vector2(-1f, 1f),
        // });

        // AddEntity(new ValueDisplay<string>(() => $"Pos: {player.Position.X}, {player.Position.Y}") {
        //     Origin = Vector2.UnitY,
        //     Position = new Vector2(0f, -260f),
        //     CharacterSize = 40f,
        //     Color = Color4.White,
        //     IsUI = true,
        //     UIAlignment = new Vector2(-1f, 1f),
        // });
    }


    public override void OnDisconnect() {
        if (Game.Settings.UseSteam) Game.Network.DisconnectSteam();
        else Game.Network.Disconnect();

        Log.Warn("Opponent disconnected");

        Game.Scenes.ChangeScene<MainScene>();
    }
}