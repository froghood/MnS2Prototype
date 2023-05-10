using SFML.Graphics;
using SFML.System;

namespace Touhou.Objects {
    public class PointHitbox : Hitbox {
        public PointHitbox(Entity entity, Vector2f offset) : base(entity, offset) { }



        public override bool Check(PointHitbox other) {
            return other.Position == Position;
        }

        public override bool Check(CircleHitbox other) {
            var xDiff = other.Position.X - Position.X;
            var yDiff = other.Position.Y - Position.Y;
            return (xDiff * xDiff + yDiff * yDiff < other.Radius * other.Radius);
        }

        public override bool Check(RectangleHitbox other) {
            throw new NotImplementedException();
        }

        public override FloatRect GetBounds() {
            return new FloatRect(Position.X, Position.Y, 0f, 0f);
        }
    }
}