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

using Color = SFML.Graphics.Color;
using Debug = Touhou.Debugging.Debug;

namespace Touhou;



internal static class Game {

    public static int MICROSECOND = 1000000;

    public static Color ClearColor { get; set; }
    public static InputManager Input { get => inputManager; }
    public static RenderWindow Window { get; }
    public static Network Network { get => network; }
    public static SceneManager Scenes { get => sceneManager; }
    public static SoundPlayer Sounds { get => soundPlayer; }



    public static Camera Camera { get; private set; }



    public static Settings Settings { get; }
    public static Random Random { get; }
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
    private static SpriteAtlas spriteAtlas = new();
    public static ShaderLibrary shaderLibrary = new();

    private static Clock clock = new();
    private static Time previousTime;

    private static Queue<Action> commandBuffer = new();
    private static SoundPlayer soundPlayer;


    private static Queue<(Drawable Drawable, TShader Shader)>[] layers = new Queue<(Drawable Drawable, TShader Shader)>[16];


    public static float AspectRatio { get => (float)Window.Size.X / Window.Size.Y; }
    private static readonly int uHDRes = 2160;

    static Game() {

        Settings = new Settings("./Settings.json");

        Random = new Random();

        soundPlayer = new SoundPlayer();



        Window = new RenderWindow(new VideoMode(1280, 720), "mns2", Styles.Default, new ContextSettings(0, 0, 16));

        System.Console.WriteLine(Window.Settings.MinorVersion);
        System.Console.WriteLine(Window.Settings.MajorVersion);

        Camera = new Camera(new Vector2f(1600f, 900f));

        DefaultFont = new Font("assets/fonts/arial.ttf");
    }

    public static void Init(string[] args) {

        spriteAtlas.Load("./assets/sprites/sprites.png", "./assets/sprites/data.json");
        shaderLibrary.Load("./assets/shaders");
        soundPlayer.Load("./assets/sounds");

        Window.SetTitle("MNS2");
        Window.SetKeyRepeatEnabled(false);
        Window.SetVerticalSyncEnabled(true);
        //Window.SetFramerateLimit(10);

        Window.Closed += (_, _) => Window.Close();
        Window.Resized += (_, _) => {
            Window.SetView(new View(new FloatRect(0f, 0f, Window.Size.X, Window.Size.Y)));
        };

        for (int i = 0; i < layers.Length; i++) {
            layers[i] = new Queue<(Drawable Drawable, TShader Shader)>();
        }

        inputManager.AttachEvents(Window);

        inputManager.ActionPressed += (actionData) => sceneManager.Current.Press(actionData);
        inputManager.ActionReleased += (action) => sceneManager.Current.Release(action);

        network.PacketReceived += (packet, endPoint) => sceneManager.Current.Receive(packet, endPoint);

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
            // foreach (var layer in layers) {
            //     layer.Clear(Color.Transparent);
            // }
            //Window.SetView(view);
            sceneManager.Current.Render();
            //sceneManager.Current.DebugRender();

            inputManager.Render();

            foreach (var layer in layers) {
                while (layer.Count > 0) {

                    var element = layer.Dequeue();

                    //System.Console.WriteLine((float)Window.Size.Y / Camera.UHDFullSize.Y);
                    //transform.Scale(new Vector2f(1f, 1f) * ((float)Window.Size.Y / Camera.UHDFullSize.Y));

                    if (element.Shader is not null && shaderLibrary.GetShader(element.Shader.Name, out var shader)) {
                        element.Shader.ApplyUniforms(shader);
                        Window.Draw(element.Drawable, new RenderStates(shader));
                    } else {
                        Window.Draw(element.Drawable);
                    }

                }
            }
            Window.Display();

            //Debug.Fields.Set("render", TimeAsFloat(clock.ElapsedTime.AsMicroseconds() - renderStartTime));

            sceneManager.Current.PostRender();

            previousTime = Time;

            while (commandBuffer.Count > 0) commandBuffer.Dequeue().Invoke();

            Camera.Position += new Vector2f(0, 0f);
        }
    }

    public static bool IsKeyPressed(Keyboard.Key key) {
        return Window.HasFocus() && Keyboard.IsKeyPressed(key);
    }

    public static bool IsActionPressed(PlayerAction action) {
        return Window.HasFocus() && inputManager.GetActionState(action);
    }



    public static void Draw(Drawable drawable, RenderStates states, int layer) {
        //layers[layer].Enqueue((drawable, states.BlendMode, states.Shader, states.Texture, states.Transform));
        //layers[layer].Draw(drawable, states);
        Window.Draw(drawable, states);
    }
    public static void Draw(Drawable drawable, int layer) {
        //layers[layer].Enqueue((drawable, null));
        //Draw(drawable, RenderStates.Default, layer);
        //layers[layer].Draw(drawable);
        Window.Draw(drawable);

        //layers[layer].Enqueue(drawable);
    }

    public static void DrawSprite(string name, SpriteStates states, TShader shader, Layers layer) {
        var sprite = spriteAtlas.GetSprite(name);
        var bounds = sprite.GetLocalBounds();

        sprite.Origin = (states.OriginType) switch {
            OriginType.Percentage => new Vector2f(bounds.Width * states.Origin.X, bounds.Height * states.Origin.Y),
            OriginType.Position => states.Origin * (states.IsUI ? Camera.GetScreenScale(true) : 1f),
            _ => states.Origin
        };

        sprite.Position = Camera.GetScreenCoords(states.Position, states.IsUI);

        sprite.Rotation = states.Rotation;
        sprite.Scale = states.Scale * Camera.GetScreenScale(states.IsUI);
        var scaleAtUHD = states.Scale * Camera.WorldToUHDScreenScale;
        if ((scaleAtUHD.X > 1f || scaleAtUHD.Y > 1f) && !states.IsUI) System.Console.WriteLine($"sprite \"{name}\" is being scaled higher than 1x (X: {scaleAtUHD.X}, Y: {scaleAtUHD.Y})");

        sprite.Color = states.Color;

        shader?.SetUniform("resolution", (Vector2f)Window.Size);
        shader?.SetUniform("position", new Vector2f() {
            X = (sprite.Position.X - sprite.Origin.X * sprite.Scale.X),
            Y = (sprite.Position.Y - sprite.Origin.Y * sprite.Scale.Y)
        });
        shader?.SetUniform("size", new Vector2f() {
            X = bounds.Width * sprite.Scale.X,
            Y = bounds.Height * sprite.Scale.Y
        });


        layers[(int)layer].Enqueue((sprite, shader));
    }

    public static void DrawSprite(string name, SpriteStates spriteStates, Layers layer) => DrawSprite(name, spriteStates, null, layer);

    public static void DrawRectangle(RectangeStates states, TShader shader, Layers layer) {
        var rectangle = new RectangleShape(states.Size * Camera.GetScreenScale(states.IsUI));

        rectangle.Origin = (states.OriginType) switch {
            OriginType.Percentage => new Vector2f(rectangle.Size.X * states.Origin.X, rectangle.Size.Y * states.Origin.Y),
            OriginType.Position => states.Origin * (states.IsUI ? Camera.GetScreenScale(true) : 1f),
            _ => states.Origin
        };

        rectangle.Position = Camera.GetScreenCoords(states.Position, states.IsUI);
        rectangle.Rotation = states.Rotation;
        rectangle.Scale = states.Scale;
        rectangle.FillColor = states.FillColor;
        rectangle.OutlineColor = states.OutlineColor;
        rectangle.OutlineThickness = states.IsUI ? states.OutlineThickness : states.OutlineThickness;

        shader?.SetUniform("resolution", (Vector2f)Window.Size);
        shader?.SetUniform("position", new Vector2f() {
            X = (rectangle.Position.X - rectangle.Origin.X * rectangle.Scale.X),
            Y = (rectangle.Position.Y - rectangle.Origin.Y * rectangle.Scale.Y)
        });
        shader?.SetUniform("size", new Vector2f() {
            X = rectangle.Size.X * rectangle.Scale.X,
            Y = rectangle.Size.Y * rectangle.Scale.Y
        });

        layers[(int)layer].Enqueue((rectangle, shader));
    }

    public static void DrawRectangle(RectangeStates states, Layers layer) => DrawRectangle(states, null, layer);

    public static void DrawCircle(CircleStates states, TShader shader, Layers layer) {
        var circle = new CircleShape(states.Radius * Camera.GetScreenScale(states.IsUI));

        circle.Origin = (states.OriginType) switch {
            OriginType.Percentage => new Vector2f(circle.Radius * 2f * states.Origin.X, circle.Radius * 2f * states.Origin.Y),
            OriginType.Position => states.Origin * (states.IsUI ? Camera.GetScreenScale(true) : 1f),
            _ => states.Origin
        };

        circle.Position = Camera.GetScreenCoords(states.Position, states.IsUI);
        circle.Rotation = states.Rotation;
        circle.Scale = states.Scale;
        circle.FillColor = states.FillColor;
        circle.OutlineColor = states.OutlineColor;
        circle.OutlineThickness = states.IsUI ? states.OutlineThickness : states.OutlineThickness;

        shader?.SetUniform("resolution", (Vector2f)Window.Size);
        shader?.SetUniform("position", new Vector2f() {
            X = (circle.Position.X - circle.Origin.X) * circle.Scale.X,
            Y = (circle.Position.Y - circle.Origin.Y) * circle.Scale.Y
        });
        shader?.SetUniform("size", new Vector2f() {
            X = circle.Radius * 2f * circle.Scale.X,
            Y = circle.Radius * 2f * circle.Scale.Y
        });

        layers[(int)layer].Enqueue((circle, shader));
    }

    public static void DrawCircle(CircleStates states, Layers layer) => DrawCircle(states, null, layer);

    public static void DrawText(string str, Font font, TextStates states, TShader shader, Layers layer) {
        // System.Console.WriteLine(Camera.GetScreenScale(states.IsUI));

        // float scaledCharacterSize = states.CharacterSize * Camera.GetScreenScale(states.IsUI);
        // uint realCharacterSize = (uint)MathF.Round(scaledCharacterSize);
        // float characterScale = scaledCharacterSize / realCharacterSize;

        var text = new Text(str, font, (uint)states.CharacterSize);

        text.Style = states.Style;

        var bounds = text.GetLocalBounds();

        text.Origin = (states.OriginType) switch {
            OriginType.Percentage => new Vector2f(bounds.Width * states.Origin.X, bounds.Height * states.Origin.Y),
            OriginType.Position => states.Origin * (states.IsUI ? Camera.GetScreenScale(true) : 1f),
            _ => states.Origin
        };

        text.Position = Camera.GetScreenCoords(states.Position, states.IsUI);
        text.Rotation = states.Rotation;
        text.Scale = states.Scale * Camera.GetScreenScale(states.IsUI);
        text.FillColor = states.FillColor;
        text.OutlineColor = states.OutlineColor;
        text.OutlineThickness = states.OutlineThickness;

        shader?.SetUniform("resolution", (Vector2f)Window.Size);
        shader?.SetUniform("position", new Vector2f() {
            X = (text.Position.X - text.Origin.X) * text.Scale.X,
            Y = (text.Position.Y - text.Origin.Y) * text.Scale.Y
        });
        shader?.SetUniform("size", new Vector2f() {
            X = bounds.Width * text.Scale.X,
            Y = bounds.Height * text.Scale.Y
        });

        layers[(int)layer].Enqueue((text, shader));
    }

    public static void DrawText(string str, Font font, TextStates states, Layers layer) => DrawText(str, font, states, null, layer);



    public static void Command(Action action) {
        commandBuffer.Enqueue(action);
    }
}