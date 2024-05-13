
using Newtonsoft.Json;



using OpenTK;
using OpenTK.Windowing.Desktop;

//using Touhou.Audio;
using Touhou.Debugging;
using Touhou.Networking;
using Touhou.Scenes;
//using Debug = Touhou.Debugging.Debug;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vector2i = OpenTK.Mathematics.Vector2i;
using Touhou.Graphics;
using Touhou.Sound;
using Steamworks;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Touhou;



internal static class Game {



    public static Renderer Renderer { get => renderer; }

    public static InputManager Input { get => inputManager; }
    private static NativeWindow window;
    public static Network Network { get => network; }
    public static SceneManager Scenes { get => sceneManager; }

    public static SoundPlayer Sounds { get => soundPlayer; }


    public static Vector2i WindowSize { get => window.ClientSize; }
    public static Camera Camera { get; private set; }



    public static Settings Settings { get; }
    public static Random Random { get; }
    public static Fields Stats { get => stats; }

    //public static Debug Debug { get; private set; } = new();


    public static Time Time { get; private set; }
    public static Time Delta { get; private set; }

    public static int FrameCount { get; private set; } = 0;

    public static string FrameTimes { get => string.Join(", ", frameTimes); }
    public static float FPS { get => 1 / (frameTimes.Sum() / frameTimes.Count); }

    //TODO: public static Font DefaultFont { get; private set; }

    private static Renderer renderer;

    private static Fields stats = new();

    private static Queue<float> frameTimes = new();
    private static InputManager inputManager = new();
    private static Network network = new();
    private static SceneManager sceneManager = new();
    private static SoundPlayer soundPlayer;

    private static Clock clock = new();
    private static Time previousTime;

    private static Queue<Action> commandBuffer = new();

    //private static SoundPlayer soundPlayer;




    public static float AspectRatio { get => (float)window.ClientSize.X / window.ClientSize.Y; }
    private static readonly int uHDRes = 2160;

    static Game() {

        Settings = new Settings("./Settings.json");

        Random = new Random();

        soundPlayer = new SoundPlayer(Settings.SoundVolume, 64, 4);
        soundPlayer.Load("./assets/sounds/hit.wav");
        soundPlayer.Load("./assets/sounds/death.wav");
        soundPlayer.Load("./assets/sounds/low_hearts.wav");
        soundPlayer.Load("./assets/sounds/graze.wav");
        soundPlayer.Load("./assets/sounds/spell.wav");
        soundPlayer.Load("./assets/sounds/bomb.wav");


        var settings = new NativeWindowSettings() {
            Size = new Vector2i(1280, 720),
            NumberOfSamples = 16,
            StartVisible = false,
        };


        window = new NativeWindow(settings);

        renderer = new Renderer(window.Context);

        Camera = new Camera(window);
        Camera.View = new Vector2(1600f, 900f);
    }

    public static void Init(string[] args) {

        if (Settings.UseSteam) SteamClient.Init(480);

        window.Title = "MNS2 OPENGL";

        window.Resize += (e) => {
            GL.Viewport(0, 0, e.Width, e.Height);
        };

        window.JoystickConnected += (e) => {
            if (e.IsConnected) {
                Log.Info($"Controller {e.JoystickId} connected");
            } else {
                Log.Warn($"Controller {e.JoystickId} disconnected");
            }
        };


        inputManager.ActionPressed += (actionData) => sceneManager.Current.Press(actionData);
        inputManager.ActionReleased += (action) => sceneManager.Current.Release(action);

        network.PacketReceived += (packet, endPoint) => sceneManager.Current.Receive(packet, endPoint);

        renderer.ClearColor = new Color4(.1f, .1f, .16f, 1f);


        Stats.Add("updateTime");
        Stats.Add("renderTime");



        foreach (var arg in args) {
            (arg switch {
                "-h" => () => Scenes.ChangeScene<HostingScene>(),
                "-c" => () => Scenes.ChangeScene<ConnectingScene>(),
                "-t" => () => Scenes.ChangeScene<OpenGLTestScene>(),
                _ => (Action)(() => Scenes.ChangeScene<MainScene>())
            })();
        }

        window.IsVisible = true;
    }

    public static void Run() {



        clock.Restart();

        while (!window.IsExiting) {


            Time = clock.Elapsed;
            Delta = Time - previousTime;

            frameTimes.Enqueue(Delta.AsSeconds());
            while (frameTimes.Count > 200 && frameTimes.TryDequeue(out var _)) ;

            var preUpdateTime = clock.Elapsed;
            Update();
            var updateTime = clock.Elapsed - preUpdateTime;
            Stats.Set("update", updateTime.AsSeconds());

            var preRenderTime = clock.Elapsed;
            Render();
            var renderTime = clock.Elapsed - preRenderTime;
            Stats.Set("render", renderTime.AsSeconds());

            PostRender();

            previousTime = Time;

            while (commandBuffer.Count > 0) commandBuffer.Dequeue().Invoke();

            FrameCount++;
        }

        //SteamClient.Shutdown();
    }

    public static void Draw(Renderable renderable, Layer layer) => renderer.Queue(renderable, layer);

    private static void Update() {



        SteamClient.RunCallbacks();

        inputManager.Process(window);

        network.Update();

        sceneManager.Current.Update();

        network.Flush();
    }

    private static void Render() {



        sceneManager.Current.Render();
        renderer.Render();
    }

    private static void PostRender() {
        sceneManager.Current.PostRender();
    }

    public static bool IsActionPressed(PlayerActions action) {
        return inputManager.IsActionPressed(window, action);
    }





    public static void Command(Action action) {
        commandBuffer.Enqueue(action);
    }

    // public static void Log(string name, string message) {
    //     File.AppendAllLines($"./log-{name}.txt", new[] { message });
    // }
}