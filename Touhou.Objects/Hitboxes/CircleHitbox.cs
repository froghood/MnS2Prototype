
using OpenTK.Mathematics;

namespace Touhou.Objects {
    public class CircleHitbox : Hitbox {

        public float Radius { get; private set; }

        public CircleHitbox(Entity entity, Vector2 offset, float radius, CollisionGroups collisionGroup, Action<Entity> collisionCallback = default(Action<Entity>)) : base(entity, offset, collisionGroup, collisionCallback) {
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
            var circleOffset = Position - other.Position;
            circleOffset *= Matrix2.Invert(other.RotationMatrix);
            var edge = other.Size / 2f;
            var xDist = MathF.Max(0f, circleOffset.X - edge.X) + MathF.Min(0f, circleOffset.X + edge.X);
            var yDist = MathF.Max(0f, circleOffset.Y - edge.Y) + MathF.Min(0f, circleOffset.Y + edge.Y);
            var dist = xDist * xDist + yDist * yDist;
            return dist < Radius * Radius;
        }

        public override Box2 GetBounds() {
            var halfSize = Vector2.One * Radius;
            return new Box2(Position - halfSize, Position + halfSize);
        }
    }
}