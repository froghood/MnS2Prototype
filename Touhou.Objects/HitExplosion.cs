using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Objects;

namespace Touhou.Objects {
    public class HitExplosion : Entity {
        private Color4 color;
        private float duration;
        private float radius;
        private float transparency;



        public HitExplosion(Vector2 position, float duration, float radius, Color4 color) {
            Position = position;
            this.duration = duration;
            this.radius = radius;
            this.color = color;
        }

        public override void Update() {

            if (LifeTime.AsSeconds() >= 1f) Destroy();

            transparency = (1f - LifeTime.AsSeconds()) * 0.5f;
        }

        public override void Render() {

            var circle = new Circle() {
                Origin = new Vector2(0.5f),
                Position = Position,
                Radius = Easing.Out(LifeTime.AsSeconds(), 5f) * radius,
                FillColor = new Color4(color.R, color.G, color.B, color.A * transparency)
            };

            Game.Draw(circle, Layer.Foreground1);

        }
    }
}