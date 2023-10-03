
using OpenTK.Mathematics;

namespace Touhou.Objects {
    public class RectangleHitbox : Hitbox {
        public Vector2 Size { get; }
        public float Rotation { get; }

        public float Cos { get; }
        public float Sin { get; }
        public Matrix2 RotationMatrix { get; }

        public RectangleHitbox(Entity entity, Vector2 offset, Vector2 size, float rotation, CollisionGroups collisionGroup, Action<Entity> collisionCallback = default(Action<Entity>)) : base(entity, offset, collisionGroup, collisionCallback) {
            Size = size;
            Rotation = rotation;

            Cos = MathF.Cos(rotation);
            Sin = MathF.Sin(rotation);
            RotationMatrix = Matrix2.CreateRotation(rotation);
        }

        public override bool Check(PointHitbox other) {
            return false;
        }

        public override bool Check(CircleHitbox other) {
            var circleOffset = other.Position - Position;
            circleOffset *= Matrix2.Invert(RotationMatrix);
            var edge = Size / 2f;
            var xDist = MathF.Max(0f, circleOffset.X - edge.X) + MathF.Min(0f, circleOffset.X + edge.X);
            var yDist = MathF.Max(0f, circleOffset.Y - edge.Y) + MathF.Min(0f, circleOffset.Y + edge.Y);
            var dist = xDist * xDist + yDist * yDist;
            return dist < other.Radius * other.Radius;
        }

        public override bool Check(RectangleHitbox other) {
            return false;
        }

        public override Box2 GetBounds() {
            var boundsSize = new Vector2(Size.X * Cos + Size.Y * Sin, Size.X * Sin + Size.Y * Cos);

            //Log.Info(boundsSize);

            return new Box2(Position - boundsSize / 2f, Position + boundsSize / 2f);
        }
    }
}