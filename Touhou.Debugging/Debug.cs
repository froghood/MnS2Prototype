
namespace Touhou.Debugging;

public class Debug {

    public Fields Fields { get; private set; } = new();

    // private CircleShape circle = new();
    // private RectangleShape rect = new();
    // private Vertex[] line = new Vertex[2];

    // public Debug() {

    // }

    // public void DrawRect(Vector2 position, Vector2 size, Color4 fill, Vector2 origin) {
    //     rect.Position = position;
    //     rect.Size = size;
    //     rect.FillColor4 = fill;
    //     rect.OutlineColor4 = Color4.Transparent;
    //     rect.Origin = origin;
    //     //Game.Draw(rect, 0);
    // }

    // public void DrawRectOutline(Vector2 position, Vector2 size, Color4 stroke, float thickness = 1f) {
    //     rect.Position = position;
    //     rect.Size = size;
    //     rect.FillColor4 = Color4.Transparent;
    //     rect.OutlineColor4 = stroke;
    //     rect.OutlineThickness = thickness;
    //     //Game.Draw(rect, 0);
    // }

    // public void DrawLine(Vector2 positionA, Vector2 positionB, Color4 stroke) {
    //     line[0] = new Vertex(positionA, stroke);
    //     line[1] = new Vertex(positionB, stroke);
    //     //Game.Window.Draw(line, PrimitiveType.Lines);
    // }
}