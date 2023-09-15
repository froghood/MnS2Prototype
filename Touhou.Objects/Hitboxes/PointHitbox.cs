
using OpenTK.Mathematics;

namespace Touhou.Objects {
    public class PointHitbox : Hitbox {
        public PointHitbox(Entity entity, Vector2 offset, CollisionGroups collisionGroup, Action<Entity> collisionCallback = default(Action<Entity>)) : base(entity, offset, collisionGroup, collisionCallback) { }



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

        public override Box2 GetBounds() {
            return new Box2(Position, Position);
        }
    }
}