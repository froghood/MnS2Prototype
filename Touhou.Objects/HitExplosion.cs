using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Objects;

namespace Touhou.Objects {
    public class HitExplosion : Entity {
        private Color4 color;
        private float duration;
        private float radius;
        private float transparency;

        private float lifeTime;

        public HitExplosion(Vector2 position, float duration, float radius, Color4 color) {
            Position = position;
            this.duration = duration;
            this.radius = radius;
            this.color = color;
        }

        public override void Update() {
            transparency = (1f - lifeTime) * 0.5f;
        }

        public override void Render() {

            var circle = new Circle() {
                Origin = new Vector2(0.5f),
                Position = Position,
                Radius = Easing.Out(lifeTime, 5f) * radius,
                FillColor = new Color4(color.R, color.G, color.B, color.A * transparency)
            };

            Game.Draw(circle, Layer.Foreground1);

            // var states = new CircleStates() {
            //     Origin = new Vector2(0.5f, 0.5f),
            //     Position = Position,
            //     Radius = Easing.Out(lifeTime, 5f) * radius,
            //     FillColor4 = new Color4(Color4.R, Color4.G, Color4.B, transparency),
            //     OutlineColor4 = Color4.Transparent,
            // };

            //Game.DrawCircle(states, 0);

        }
        public override void PostRender() {

            lifeTime = MathF.Min(lifeTime + Game.Delta.AsSeconds() / duration, 1f);

            if (lifeTime >= 1f) Destroy();
        }
    }
}