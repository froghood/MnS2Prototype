using SFML.Graphics;
using SFML.System;

namespace Touhou;

public struct TextStates {


    public float CharacterSize { get; init; }
    public Text.Styles Style { get; init; }


    public Vector2f Origin { get; init; }
    public OriginType OriginType { get; init; }


    public Vector2f Position { get; init; }
    public float Rotation { get; init; }
    public Vector2f Scale { get; init; }


    public Color FillColor { get; init; }
    public Color OutlineColor { get; init; }
    public float OutlineThickness { get; init; }


    public bool IsUI { get; init; }

    public TextStates() {
        CharacterSize = 14;
        Style = Text.Styles.Regular;
        Origin = default(Vector2f);
        OriginType = OriginType.Percentage;
        Position = default(Vector2f);
        Rotation = 0f;
        Scale = new Vector2f(1f, 1f);
        FillColor = Color.White;
        OutlineColor = Color.Black;
        OutlineThickness = 0f;
        IsUI = true;
    }

}