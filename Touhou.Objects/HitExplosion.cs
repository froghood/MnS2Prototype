using SFML.Graphics;
using SFML.System;
using Touhou.Objects;

namespace Touhou.Objects {
    public class HitExplosion : Entity {
        private Color color;
        private CircleShape circle;
        private float duration;
        private float radius;
        private byte transparency;

        private float lifeTime;

        public HitExplosion(Vector2f position, float duration, float radius, Color color) {
            Position = position;
            this.duration = duration;
            this.radius = radius;
            this.color = color;

            circle = new CircleShape();
            circle.Position = Position;
        }

        public override void Update() {
            transparency = (byte)MathF.Round((255f - lifeTime * 255f) * 0.5f);
        }

        public override void Render() {
            circle.Radius = (1f - MathF.Pow(1f - lifeTime, 5f)) * radius;
            circle.FillColor = new Color(color.R, color.G, color.B, transparency);
            circle.Origin = new Vector2f(1f, 1f) * circle.Radius;
            Game.Window.Draw(circle);
        }
        public override void PostRender() {
            lifeTime = MathF.Min(lifeTime + Game.Delta.AsSeconds() / duration, 1f);

            if (lifeTime >= 1f) Destroy();
        }
    }
}