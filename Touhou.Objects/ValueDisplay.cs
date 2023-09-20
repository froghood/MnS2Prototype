
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects;

public class ValueDisplay<T> : Entity {

    public float Depth { get => text.Depth; set => text.Depth = value; }
    public Vector2 Origin { get => text.Origin; set => text.Origin = value; }
    public float Rotation { get => text.Rotation; set => text.Rotation = value; }
    public Vector2 Scale { get => text.Scale; set => text.Scale = value; }
    public bool IsUI { get => text.IsUI; set => text.IsUI = value; }
    public Vector2 UIAlignment { get => text.Alignment; set => text.Alignment = value; }



    public float CharacterSize { get => text.CharacterSize; set => text.CharacterSize = value; }
    public string Font { get => text.Font; set => text.Font = value; }
    public float Padding { get => text.Padding; set => text.Padding = value; }
    public Color4 Color { get => text.Color; set => text.Color = value; }
    public float Boldness { get => text.Boldness; set => text.Boldness = value; }



    private readonly Func<T> valueDelegate;
    private T value;

    private Text text = new();

    public ValueDisplay(Func<T> valueDelegate) {
        this.valueDelegate = valueDelegate;
    }

    public override void Update() {
        value = valueDelegate.Invoke();
    }

    public override void Render() {
        text.Position = Position;
        text.DisplayedText = value.ToString();

        Game.Draw(text, Layers.UI1);
    }

    public override void PostRender() { }




}

// public class FloatDisplay : Entity {

//     public Color4 Color4 { get => text.FillColor4; set => text.FillColor4 = value; }
//     public uint CharacterSize { get => text.CharacterSize; set => text.CharacterSize = value; }

//     private readonly Func<float> valueDelegate;
//     private float value;

//     private Text text = new();

//     public FloatDisplay(Func<float> valueDelegate) {
//         this.valueDelegate = valueDelegate;

//         text.Font = Game.DefaultFont;
//     }

//     public override void Update() {
//         value = valueDelegate.Invoke();
//     }

//     public override void Render() {
//         text.Position = Position;
//         text.DisplayedString = $"{value}";
//         Game.Draw(text, 0);
//     }

//     public override void PostRender() { }
// }