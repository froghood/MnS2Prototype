using OpenTK.Mathematics;

using Touhou.Graphics;

namespace Touhou.Scenes;

public class OpenGLTestScene : Scene {

    private Sprite sprite;
    private Sprite sprite2;

    private Text text;

    private Rectangle rect;
    private Rectangle rect2;

    public OpenGLTestScene() {
        sprite = new Sprite("reimu");
        sprite.Scale = Vector2.One;
        sprite.Origin = Vector2.One * 0.5f;
        sprite.Depth = 1f;

        sprite2 = new Sprite("knife2");
        sprite2.Scale = Vector2.One;
        sprite2.Origin = Vector2.One * 0.5f;
        sprite2.Depth = 0.5f;

        text = new Text {
            DisplayedText = "hello",
            Font = "consolas",
            CharacterSize = 100f,
            Origin = new Vector2(0f, 0f),
            Padding = 0f,
            Color = Color4.White,
            IsUI = true,
            Alignment = new Vector2(-1, -1),
        };

        rect = new Rectangle {
            Position = new Vector2(0f, 0f),
            Size = new Vector2(100f),
            StrokeWidth = 1f,
            FillColor = Color4.Transparent,
            StrokeColor = Color4.White,
            Origin = new Vector2(0.5f, 0.5f),
            IsUI = true,
            Alignment = new Vector2(0, 0),
        };

        rect2 = new Rectangle {
            Position = new Vector2(0f, 0f),
            Size = new Vector2(200f),
            StrokeWidth = 5f,
            FillColor = Color4.Transparent,
            StrokeColor = Color4.White,
            Origin = new Vector2(0.5f, 0.5f),
            IsUI = true,
            Alignment = new Vector2(0, 0),
        };
    }

    public override void OnInitialize() {
        AddEntity(new Touhou.Objects.Generics.RenderCallback(() => {

            var bg = new Sprite("starrynight") {
                Origin = new Vector2(0.5f),
                Scale = new Vector2(6f),
                IsUI = true,
            };

            Game.Draw(bg, Layers.Background1);

            var sprite = new Sprite("blendtest") {
                Origin = new Vector2(0.5f),
                //Position = new Vector2(100f * Game.Time.AsSeconds()),
                Scale = new Vector2(6f),
                IsUI = true,
                BlendMode = BlendMode.Additive,
            };

            Game.Draw(sprite, Layers.Background1);

            //Game.Draw(rect, Layers.Foreground1);

            // Game.Draw(rect2, Layers.Foreground1);

            //Game.Draw(text, Layers.Foreground1);

            // Game.Draw(sprite);
            // sprite.Rotation += 0.1f * Game.Delta.AsSeconds();

            // Game.Draw(sprite2);
            // sprite2.Rotation -= 0.1f * Game.Delta.AsSeconds();

            // text.DisplayText = $"{String.Format("{0:#,0.0}", Game.Time.AsSeconds())}";
            // Game.Draw(text);
            // //text.Rotation += 0.05f * Game.Delta.AsSeconds();
            // //text.Padding = (MathF.Sin(Game.Time.AsSeconds() * MathF.PI) * 0.5f + 0.5f) * 0.2f;
            // text.Boldness = (MathF.Sin(Game.Time.AsSeconds() * MathF.Tau * 2f) * 0.5f + 0.5f);

            // System.Console.WriteLine(Game.WindowSize);

            //rect.Rotation = Game.Time.AsSeconds();
        }));
    }


}