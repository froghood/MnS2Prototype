
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects {
    public class RectangleHitbox : Hitbox {
        public Vector2 Size { get; }
        public float Rotation { get; }

        public float Cos { get; }
        public float Sin { get; }
        public Matrix2 RotationMatrix { get; }

        public RectangleHitbox(Entity entity, Vector2 offset, Vector2 size, float rotation, CollisionGroup collisionGroup, Action<Entity, Hitbox> collisionCallback = default) : base(entity, offset, collisionGroup, collisionCallback) {
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
            var boundsSize = new Vector2(Size.X * MathF.Abs(Cos) + Size.Y * MathF.Abs(Sin), Size.X * MathF.Abs(Sin) + Size.Y * MathF.Abs(Cos));

            //Log.Info(boundsSize);

            return new Box2(Position - boundsSize / 2f, Position + boundsSize / 2f);
        }

        public override void Render() {
            Game.Draw(new Rectangle {
                Origin = new Vector2(0.5f),
                Position = Position,
                Size = Size,
                Rotation = Rotation,
                StrokeWidth = 1f,
                StrokeColor = Entity.CanCollide ? Color4.Red : Color4.White,
                FillColor = Color4.Transparent
            }, Layer.Foreground2);

            // var bounds = GetBounds();

            // Game.Draw(new Rectangle {
            //     Origin = new Vector2(0.5f),
            //     Position = bounds.Center,
            //     Size = bounds.Size,
            //     StrokeWidth = 1f,
            //     StrokeColor = Color4.SkyBlue,
            //     FillColor = Color4.Transparent,
            // }, Layers.Foreground2);
        }
    }
}