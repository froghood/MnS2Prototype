using SFML.Graphics;
using SFML.System;

namespace Touhou.Objects {
    public class CircleHitbox : Hitbox {

        public float Radius { get; private set; }

        public CircleHitbox(Entity entity, Vector2f offset, float radius) : base(entity, offset) {
            Radius = radius;
        }

        public override bool Check(PointHitbox other) {
            var xDiff = other.Position.X - Position.X;
            var yDiff = other.Position.Y - Position.Y;
            return (xDiff * xDiff + yDiff * yDiff < this.Radius * this.Radius);
        }

        public override bool Check(CircleHitbox other) {
            var xDist = other.Position.X - this.Position.X;
            var yDist = other.Position.Y - this.Position.Y;
            var dist = xDist * xDist + yDist * yDist;
            return (dist < MathF.Pow(other.Radius + this.Radius, 2f));
        }

        public override bool Check(RectangleHitbox other) {
            throw new NotImplementedException();
        }

        public override FloatRect GetBounds() {
            return new FloatRect(Position.X - Radius, Position.Y - Radius, Radius * 2f, Radius * 2f);
        }
    }
}