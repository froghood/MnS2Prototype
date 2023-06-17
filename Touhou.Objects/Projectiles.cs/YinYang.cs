using SFML.Graphics;
using SFML.System;

namespace Touhou.Objects.Projectiles;

public class YinYang : ParametricProjectile {


    public float Velocity { get; init; }

    private float radius;

    private CircleShape circle;
    private CircleShape hitboxShape;
    private RectangleShape boundsShape;

    public YinYang(Vector2f origin, float direction, bool isRemote, float radius, Time spawnTimeOffset = default(Time)) : base(origin, direction, isRemote, spawnTimeOffset) {


        circle = new CircleShape();
        this.radius = radius;

        hitboxShape = new CircleShape();
        hitboxShape.FillColor = Color.Transparent;
        hitboxShape.OutlineColor = new Color(0, 255, 0);
        hitboxShape.OutlineThickness = 1f;

        boundsShape = new RectangleShape();
        boundsShape.FillColor = Color.Transparent;
        boundsShape.OutlineColor = new Color(0, 200, 255);
        boundsShape.OutlineThickness = 1f;

        CollisionType = CollisionType.Projectile;
        Hitboxes.Add(new CircleHitbox(this, -new Vector2f(0f, 0f), this.radius, (_) => { }));
    }

    public override void Render() {
        circle.Radius = radius + 5f;
        circle.Origin = new Vector2f(circle.Radius, circle.Radius);
        circle.FillColor = Color;
        circle.Position = Position;

        var state = new SpriteStates() {
            Origin = new Vector2f(0.5f, 0.5f),
            Position = Position,
            Rotation = CurrentTime * Velocity / (MathF.Tau * radius) * -360f,
            Scale = new Vector2f(0.25f, 0.25f) * radius / 60f
        };

        var shader = new TShader("projectileColor");
        shader.SetUniform("color", Color);

        Game.DrawSprite("yinyang", state, shader, Layers.BackgroundProjectiles1);

        //Game.Draw(circle, 0);
    }

    public override void DebugRender() {
        foreach (CircleHitbox hitbox in Hitboxes) {

            var states = new CircleStates() {
                Origin = new Vector2f(0.5f, 0.5f),
                Position = hitbox.Position,
                Radius = hitbox.Radius,
                FillColor = Color.Transparent,
                OutlineColor = Color.White,
            };

            Game.DrawCircle(states, Layers.UI2);

            // hitboxShape.Position = hitbox.Position;
            // hitboxShape.Radius = hitbox.Radius;
            // hitboxShape.Origin = new Vector2f(hitbox.Radius, hitbox.Radius);
            // Game.Draw(hitboxShape, 0);

            // var bounds = hitbox.GetBounds();
            // boundsShape.Position = new Vector2f(bounds.Left, bounds.Top);
            // boundsShape.Size = new Vector2f(bounds.Width, bounds.Height);
            // Game.Draw(boundsShape, 0);


        }
    }

    protected override float FuncX(float t) {
        // float t = MathF.Max(time - 0.15f, 0f);
        return Velocity * t;
    }

    protected override float FuncY(float time) {
        return 0f;
    }

    protected override Vector2f Adjust(float time, Vector2f position) {

        position += Match.Bounds;

        var matchBoundsX = Match.Bounds.X * 2f;
        var matchBoundsY = Match.Bounds.Y * 2f;

        var screenOffset = new Vector2f(
            MathF.Abs(MathF.Floor(position.X / matchBoundsX)),
            MathF.Abs(MathF.Floor(position.Y / matchBoundsY))
        );

        return new Vector2f(
            (screenOffset.X % 2 * matchBoundsX + TMathF.Mod(position.X, matchBoundsX) * MathF.Pow(-1, screenOffset.X)) - Match.Bounds.X,
            (screenOffset.Y % 2 * matchBoundsY + TMathF.Mod(position.Y, matchBoundsY) * MathF.Pow(-1, screenOffset.Y)) - Match.Bounds.Y
        );
    }
}