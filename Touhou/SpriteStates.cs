using SFML.Graphics;
using SFML.System;

namespace Touhou;

public readonly struct SpriteStates {

    public Vector2f Origin { get; init; }
    public OriginType OriginType { get; init; }
    public Vector2f Position { get; init; }
    public float Rotation { get; init; }
    public Vector2f Scale { get; init; }
    public Color Color { get; init; }
    public bool IsUI { get; init; }

    public SpriteStates() {
        Origin = default(Vector2f);
        OriginType = OriginType.Percentage;
        Position = default(Vector2f);
        Rotation = 0f;
        Scale = new Vector2f(1f, 1f);
        Color = Color.White;
        IsUI = false;
    }
}