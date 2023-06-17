using SFML.Graphics;

namespace Touhou.Objects;

public class ValueDisplay<T> : Entity {

    public Color Color { get => text.FillColor; set => text.FillColor = value; }
    public uint CharacterSize { get => text.CharacterSize; set => text.CharacterSize = value; }

    private readonly Func<T> valueDelegate;
    private T value;

    private Text text = new();

    public ValueDisplay(Func<T> valueDelegate) {
        this.valueDelegate = valueDelegate;
        text.Font = Game.DefaultFont;
    }

    public override void Update() {
        value = valueDelegate.Invoke();
    }

    public override void Render() {
        text.Position = Position;
        text.DisplayedString = value.ToString();
        Game.Draw(text, 0);
    }

    public override void PostRender() { }




}

// public class FloatDisplay : Entity {

//     public Color Color { get => text.FillColor; set => text.FillColor = value; }
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