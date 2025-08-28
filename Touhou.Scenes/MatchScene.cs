

using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Objects;
using Touhou.Objects.Characters;
using Touhou.Objects.Generics;
using Touhou.Objects.Projectiles;

namespace Touhou.Scenes;

public class NetplayMatchTScene : TScene {

    private readonly bool isP1;
    private readonly Time startTime;

    private readonly Graph updateTimeGraph;
    private readonly Graph renderTimeGraph;

    private readonly CharacterOption localOption;
    private readonly CharacterOption remoteOption;
    private readonly Character localCharacter;
    private readonly Character remoteCharacter;


    //private Action<Time> latencyGraphDelegate;

    public NetplayMatchTScene(bool isP1, Time startTime, CharacterOption localOption, CharacterOption remoteOption) {

        this.isP1 = isP1;
        this.startTime = startTime;
        this.localOption = localOption;
        this.remoteOption = remoteOption;

        updateTimeGraph = new Graph(() => {
            Game.Get<Stats>().TryGet("update", out var value);
            return value;
        }, 5000) {
            Size = new Vector2(800f, 100f),
            Origin = Vector2.UnitX * 0.5f,
            Color = Color4.Green,
            IsUI = true,
            Alignment = new Vector2(0.42f, -1f),
        };

        renderTimeGraph = new Graph(() => {
            Game.Get<Stats>().TryGet("render", out var value);
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

        Projectile.TotalLocalProjectiles = 0;
        Projectile.TotalRemoteProjectiles = 0x80000000;

        var localCharacter = localOption.GetCharacter(isP1, true, new Color4(0.8f, 1f, 0.8f, 1f));
        var remoteCharacter = remoteOption.GetCharacter(!isP1, false, new Color4(1f, 0.8f, 0.8f, 1f));

        var match = new NetplayMatch(isP1, startTime, localOption, remoteOption, localCharacter, remoteCharacter);


        AddEntity(match);
        AddEntity(localCharacter.GetController(ControllerType.LocalNetplay));
        AddEntity(remoteCharacter.GetController(ControllerType.RemoteNetplay));
        AddEntity(localCharacter);
        AddEntity(remoteCharacter);



        AddEntity(new RenderCallback(() => {

            var matchBoundsRectangle = new Rectangle() {
                Origin = new Vector2(0.5f, 0.5f),
                Size = match.Bounds * 2f,
                FillColor = Color4.Transparent,
                StrokeColor = new Color4(255, 255, 255, 60),
                StrokeWidth = 1f,
            };

            Game.Get<Renderer>().Queue(matchBoundsRectangle, Layer.Background2);




            // Game.Get<Renderer>().Queue(updateTimeGraph, Layers.UI1);
            // Game.Get<Renderer>().Queue(renderTimeGraph, Layers.UI1);

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

            //         Game.Get<Renderer>().Queue(rect, Layers.UI1);

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
            // Game.Get<Renderer>().Queue(localProjectileHistroyDisplay, Layers.UI1);

            // var remoteProjectileHistroyDisplay = new ProjectileHistoryDisplay("remote", Projectile.RemoteProjectileHistory) {
            //     Origin = new Vector2(0f, 1f),
            //     Size = new Vector2(2600f, 80f),
            //     FillColor = new Color4(0f, 0f, 0f, 0.5f),
            //     StrokeColor = Color4.White,
            //     StrokeWidth = 1f,
            //     IsUI = true,
            //     Alignment = new Vector2(-0.9f, 0.905f),
            // };
            // Game.Get<Renderer>().Queue(remoteProjectileHistroyDisplay, Layers.UI1);


        }));

        Game.Get<Network>().ResetPing();
        if (isP1) Game.Get<Network>().StartLatencyCorrection();

        //var player = new PlayerReimu() { Position = new Vector2(80f, Game.Window.Size.Y / 2f) };

        //AddEntity(player);
        //AddEntity(opponent);

        AddEntity(new UpdateCallback(() => {

            var distance = MathF.Sqrt(
                MathF.Pow(remoteCharacter.Position.X - localCharacter.Position.X, 2f) +
                MathF.Pow(remoteCharacter.Position.Y - localCharacter.Position.Y, 2f)
            );

            float zoom = MathF.Max(MathF.Min((distance - 250f) / 750f, 1f), 0f);

            var targetView = new Vector2(1600f, 900f) * (0.9f + 0.1f * zoom);
            //Game.Get<Camera>().View += (targetView - Game.Get<Camera>().View) * (1f - MathF.Pow(0.05f, Game.Delta.AsSeconds()));

            var targetPosition = (localCharacter.Position + remoteCharacter.Position) / new Vector2(3f, 9f);
            Game.Get<Camera>().Position += (targetPosition - Game.Get<Camera>().Position) * (1f - MathF.Pow(0.05f, Game.Delta.AsSeconds()));
        }));

        AddEntity(new NetplayMatchUI(isP1));


        //Game.Get<Network>().DataReceived += latencyGraphDelegate;

        //AddEntity(latencyGraph);

        // AddEntity(new ValueDisplay<string>(() => $"FPS: {Game.FPS}") {
        //     Origin = Vector2.UnitY,
        //     Color = Color4.White,
        //     CharacterSize = 40f,
        //     IsUI = true,
        //     UIAlignment = new Vector2(0.75f, 1f),
        // });

        // AddEntity(new ValueDisplay<string>(() => $"Network Time: {Game.Get<Network>().Time.AsMilliseconds()}") {
        //     Origin = Vector2.UnitY,
        //     Position = new Vector2(0f, -70f),
        //     CharacterSize = 40f,
        //     Color = Color4.White,
        //     IsUI = true,
        //     UIAlignment = new Vector2(-1f, 1f),
        // });

        // AddEntity(new ValueDisplay<string>(() => $"Lat: {Game.Get<Network>().PerceivedLatency.AsMilliseconds()}") {
        //     Origin = Vector2.UnitY,
        //     Position = new Vector2(0f, -140f),
        //     CharacterSize = 40f,
        //     Color = Color4.White,
        //     IsUI = true,
        //     UIAlignment = new Vector2(-1f, 1f),
        // });

        // AddEntity(new ValueDisplay<string>(() => $"Their Lat: {Game.Get<Network>().TheirPerceivedLatency.AsMilliseconds()}") {
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
        if (Game.Get<Settings>().UseSteam) Game.Get<Network>().DisconnectSteam();
        else Game.Get<Network>().Disconnect();

        Log.Warn("Opponent disconnected");

        Game.Get<SceneManager>().ChangeScene<MainTScene>();
    }
}