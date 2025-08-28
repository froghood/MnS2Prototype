
global using Touhou;
global using Touhou.Networking;
global using Touhou.Objects;
global using Touhou.Scenes;
global using Touhou.Graphics;
global using Touhou.Debugging;

global using OpenTK.Graphics.OpenGL4;
global using FrogLib;

using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using Touhou.Sound;
using OsuEditor;


public class Program {
    private static void Main(string[] args) {

        FrogLib.Log.Info("Starting Touhou...");

        var windowSettings = new NativeWindowSettings() {
            ClientSize = new Vector2i(1280, 720),
            NumberOfSamples = 16,
            StartVisible = false,
        };

        Game.Init(windowSettings);

        Game.Register<Settings>().Load("./Settings.json");
        Game.Register<Stats>();

        Game.Register<CommandService>();
        Game.Register<Input>();
        Game.Register<Network>();
        Game.Register<SceneManager>();

        var soundPlayer = Game.Register<SoundPlayer>();
        soundPlayer.Load("./assets/sounds/hit.wav");
        soundPlayer.Load("./assets/sounds/death.wav");
        soundPlayer.Load("./assets/sounds/low_hearts.wav");
        soundPlayer.Load("./assets/sounds/graze.wav");
        soundPlayer.Load("./assets/sounds/spell.wav");
        soundPlayer.Load("./assets/sounds/bomb.wav");

        Game.Register<BindlessTextureLibrary>();
        //Game.Register<TShaderLibrary>();
        Game.Register<Camera>();
        Game.Register<Renderer>();

        try {
            Game.Run(RunType.VariableCoupled);
        } catch (Exception e) {
            Touhou.Log.Error(e);
            Console.ReadLine();
        }


        // var window = new NativeWindow(settings);



        // try {
        //     Game.Init(args);
        //     Game.Run();
        // } catch (Exception e) {
        //     Log.Error(e);
        //     Console.ReadLine();
        // }

    }
}