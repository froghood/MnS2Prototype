using SFML.Graphics;
using SFML.System;

namespace Touhou.Objects {
    public class RectangleHitbox : Hitbox {
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Angle { get; private set; }

        public RectangleHitbox(Entity entity, Vector2f offset, float width, float height, float angle) : base(entity, offset) {
            Width = width;
            Height = height;
            Angle = angle;
        }

        public override bool Check(PointHitbox other) {
            return false;
        }

        public override bool Check(CircleHitbox other) {
            return false;
        }

        public override bool Check(RectangleHitbox other) {
            return false;
        }

        public override FloatRect GetBounds() {
            var sin = MathF.Abs(MathF.Sin(Angle));
            var cos = MathF.Abs(MathF.Cos(Angle));
            var boundsSize = new Vector2f(Width * sin + Height * cos, Height * sin + Width * cos);
            return new FloatRect(Offset - boundsSize / 2f, boundsSize);
        }
    }
}