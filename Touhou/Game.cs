using System.Diagnostics;
using Newtonsoft.Json;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Touhou.Audio;
using Touhou.Debugging;
using Touhou.Net;
using Touhou.Scenes;
using Touhou.Scenes.Main;
using Color = SFML.Graphics.Color;
using Debug = Touhou.Debugging.Debug;

namespace Touhou;



internal static class Game {

    public static int MICROSECOND = 1000000;

    public static Color ClearColor { get; set; }
    public static InputManager Input { get => inputManager; }
    public static RenderWindow Window { get; private set; }
    public static Network Network { get => network; }
    public static SceneManager Scenes { get => sceneManager; }
    public static SoundPlayer Sounds { get => soundPlayer; }

    public static Settings Settings { get; private set; }
    public static Fields Stats { get => stats; }
    public static Debug Debug { get; private set; } = new();


    public static Time Time { get; private set; }
    public static Time Delta { get; private set; }

    public static string FrameTimes { get => string.Join(", ", frameTimes); }
    public static float FPS { get => 1 / (frameTimes.Sum() / frameTimes.Count); }

    public static Font DefaultFont { get; private set; }

    private static Fields stats = new();

    private static Queue<float> frameTimes = new();
    private static InputManager inputManager = new();
    private static Network network = new();
    private static SceneManager sceneManager = new();
    //private static SpriteAtlas spriteAtlas = new();

    private static Clock clock = new();
    private static Time previousTime;

    private static CircleShape circle = new();
    private static RectangleShape rectangle = new();

    private static View view;
    private static Queue<Action> commandBuffer = new();
    private static SoundPlayer soundPlayer;

    static Game() {

        Settings = new Settings("./Settings.json");

        soundPlayer = new SoundPlayer();

        Window = new RenderWindow(new VideoMode(1280, 720), "mns2", Styles.Close, new ContextSettings(24, 8, 16));
        DefaultFont = new Font("assets/arial.ttf");

        view = new View(new Vector2f(0f, 0f), new Vector2f(1280f, 720f));
    }

    public static void Init(string[] args) {

        soundPlayer.Load("./assets/sounds");

        //spriteAtlas.LoadSprites("./assets/sprites");

        Window.SetTitle("MNS2");
        Window.SetKeyRepeatEnabled(false);
        Window.SetVerticalSyncEnabled(true);
        //Window.SetFramerateLimit(10);

        Window.Closed += (_, _) => Window.Close();

        inputManager.AttachEvents(Window);

        inputManager.ActionPressed += (actionData) => sceneManager.Current.Press(actionData);
        inputManager.ActionReleased += (action) => sceneManager.Current.Release(action);

        network.PacketReceived += (packet, endPoint) => sceneManager.Current.Receive(packet, endPoint);

        // Stats.BackgroundColor = new Color(0, 0, 0, 80);
        // Stats.Size = new Vector2f(300f, 150f);
        // Stats.Position = new Vector2f(0f, Game.Window.Size.Y - Stats.Size.Y);
        // Stats.TextSpacing = 20f;
        // Stats.AddGraph("frame", 100, 1f, new Color(0, 100, 200), false);

        Debug.Fields.Add("step");
        Debug.Fields.Add("update");
        Debug.Fields.Add("collision");
        Debug.Fields.Add("render");
        Debug.Fields.Add("postRender");

        Scenes.PushScene<MainScene>();
    }

    public static void Run() {



        clock.Restart();

        while (Window.IsOpen) {

            Time = clock.ElapsedTime;
            Delta = Time - previousTime;

            var deltaAsFloat = Delta.AsSeconds();

            Debug.Fields.Set("step", deltaAsFloat);

            //Stats.AddSample("frame", delta);

            frameTimes.Enqueue(deltaAsFloat);
            while (frameTimes.Count > 200 && frameTimes.TryDequeue(out var overflow)) ;

            network.Update();
            inputManager.Update(Window);
            //Window.DispatchEvents();

            var updateStartTime = clock.ElapsedTime.AsMicroseconds();
            sceneManager.Current.Update();
            //Debug.Fields.Set("update", TimeAsFloat(clock.ElapsedTime.AsMicroseconds() - updateStartTime));

            var renderStartTime = clock.ElapsedTime.AsMicroseconds();
            Window.Clear(ClearColor);
            //Window.SetView(view);
            sceneManager.Current.Render();
            //_sceneManager.Current.DebugRender(Time, deltaAsFloat);

            inputManager.Render();
            Window.Display();

            //Debug.Fields.Set("render", TimeAsFloat(clock.ElapsedTime.AsMicroseconds() - renderStartTime));

            sceneManager.Current.PostRender();

            previousTime = Time;

            while (commandBuffer.Count > 0) commandBuffer.Dequeue().Invoke();
        }
    }

    public static bool IsKeyPressed(Keyboard.Key key) {
        return Window.HasFocus() && Keyboard.IsKeyPressed(key);
    }

    public static bool IsActionPressed(PlayerAction action) {
        return Window.HasFocus() && inputManager.GetActionState(action);
    }

    public static void DrawSprite(Drawable drawable, int layer) {

    }

    public static void Command(Action action) {
        commandBuffer.Enqueue(action);
    }
}