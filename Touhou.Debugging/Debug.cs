using SFML.Graphics;
using SFML.System;

namespace Touhou.Debugging;

public class Debug {

    public Fields Fields { get; private set; } = new();

    private CircleShape circle = new();
    private RectangleShape rect = new();
    private Vertex[] line = new Vertex[2];

    public Debug() {

    }

    public void DrawRect(Vector2f position, Vector2f size, Color fill, Vector2f origin) {
        rect.Position = position;
        rect.Size = size;
        rect.FillColor = fill;
        rect.OutlineColor = Color.Transparent;
        rect.Origin = origin;
        Game.Draw(rect, 0);
    }

    public void DrawRectOutline(Vector2f position, Vector2f size, Color stroke, float thickness = 1f) {
        rect.Position = position;
        rect.Size = size;
        rect.FillColor = Color.Transparent;
        rect.OutlineColor = stroke;
        rect.OutlineThickness = thickness;
        Game.Draw(rect, 0);
    }

    public void DrawLine(Vector2f positionA, Vector2f positionB, Color stroke) {
        line[0] = new Vertex(positionA, stroke);
        line[1] = new Vertex(positionB, stroke);
        Game.Window.Draw(line, PrimitiveType.Lines);
    }
}