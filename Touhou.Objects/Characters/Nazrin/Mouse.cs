using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class Mouse : Entity {



    public void SetPosition(Vector2 newPosition, bool interpolate = false) {
        Position = newPosition;
    }

    public override void Render() {
        var circle = new Circle() {
            Radius = 10f,
            FillColor = Color4.Transparent,
            StrokeColor = Color4.White,
            StrokeWidth = 1f,
            Origin = new Vector2(0.5f),
            Position = Position
        };

        Game.Draw(circle, Layer.Foreground1);


    }


}