

global using Touhou;
global using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using Touhou.Sound;
using FrogLib;
using OsuEditor;
using Touhou.Graphics;
using Touhou.Networking;

public class Program {
    private static void Main(string[] args) {

        var settings = new NativeWindowSettings() {
            ClientSize = new Vector2i(1280, 720),
            NumberOfSamples = 16,
            StartVisible = false,
        };

        FrogLib.Game.Init(settings);

        FrogLib.Game.Register<Settings>();
        FrogLib.Game.Register<Input>();
        FrogLib.Game.Register<Network>();
        FrogLib.Game.Register<SceneStorage>();

        var soundPlayer = FrogLib.Game.Register<SoundPlayer>();
        soundPlayer.Load("./assets/sounds/hit.wav");
        soundPlayer.Load("./assets/sounds/death.wav");
        soundPlayer.Load("./assets/sounds/low_hearts.wav");
        soundPlayer.Load("./assets/sounds/graze.wav");
        soundPlayer.Load("./assets/sounds/spell.wav");
        soundPlayer.Load("./assets/sounds/bomb.wav");

        FrogLib.Game.Register<BindlessTextureLibrary>();
        FrogLib.Game.Register<FrogLib.ShaderLibrary>();
        FrogLib.Game.Register<Renderer>();

        try {
            FrogLib.Game.Run(RunType.VariableCoupled);
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