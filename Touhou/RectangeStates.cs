using SFML.Graphics;
using SFML.System;

namespace Touhou;
public struct RectangeStates {

    public Vector2f Origin { get; init; }
    public OriginType OriginType { get; init; }
    public Vector2f Position { get; init; }
    public float Rotation { get; init; }
    public Vector2f Scale { get; init; }
    public Color FillColor { get; init; }
    public Color OutlineColor { get; init; }
    public float OutlineThickness { get; init; }
    public Vector2f Size { get; init; }
    public bool IsUI { get; init; }

    public RectangeStates() {
        Origin = default(Vector2f);
        OriginType = OriginType.Percentage;
        Position = default(Vector2f);
        Size = default(Vector2f);
        Rotation = 0f;
        Scale = new Vector2f(1f, 1f);
        FillColor = Color.White;
        OutlineColor = Color.Black;
        OutlineThickness = 1f;
        IsUI = false;
    }
}
