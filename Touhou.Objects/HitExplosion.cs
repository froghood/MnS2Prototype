using SFML.Graphics;
using SFML.System;
using Touhou.Objects;

namespace Touhou.Objects {
    public class HitExplosion : Entity {
        private Color color;
        private float duration;
        private float radius;
        private byte transparency;

        private float lifeTime;

        public HitExplosion(Vector2f position, float duration, float radius, Color color) {
            Position = position;
            this.duration = duration;
            this.radius = radius;
            this.color = color;
        }

        public override void Update() {
            transparency = (byte)MathF.Round((255f - lifeTime * 255f) * 0.5f);
        }

        public override void Render() {
            var states = new CircleStates() {
                Origin = new Vector2f(0.5f, 0.5f),
                Position = Position,
                Radius = Easing.Out(lifeTime, 5f) * radius,
                FillColor = new Color(color.R, color.G, color.B, transparency),
                OutlineColor = Color.Transparent,
            };

            Game.DrawCircle(states, 0);

        }
        public override void PostRender() {
            lifeTime = MathF.Min(lifeTime + Game.Delta.AsSeconds() / duration, 1f);

            if (lifeTime >= 1f) Destroy();
        }
    }
}