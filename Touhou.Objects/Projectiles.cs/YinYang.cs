using SFML.Graphics;
using SFML.System;

namespace Touhou.Objects;

public class YinYang : Projectile {


    public float Velocity { get; init; }
    private float radius;

    private CircleShape circle;
    private CircleShape hitboxShape;
    private RectangleShape boundsShape;

    public YinYang(Vector2f origin, float direction, float radius, Time spawnTimeOffset = default(Time)) : base(origin, direction, spawnTimeOffset) {


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
        Hitboxes.Add(new CircleHitbox(this, -new Vector2f(0f, 0f), this.radius));
    }

    public override void Render(Time time, float delta) {
        circle.Radius = radius + 5f;
        circle.Origin = new Vector2f(circle.Radius, circle.Radius);
        circle.FillColor = Color;
        circle.Position = Position;

        Game.Window.Draw(circle);
    }

    public override void DebugRender(Time time, float delta) {
        foreach (CircleHitbox hitbox in Hitboxes) {
            hitboxShape.Position = hitbox.Position;
            hitboxShape.Radius = hitbox.Radius;
            hitboxShape.Origin = new Vector2f(hitbox.Radius, hitbox.Radius);
            Game.Window.Draw(hitboxShape);

            var bounds = hitbox.GetBounds();
            boundsShape.Position = new Vector2f(bounds.Left, bounds.Top);
            boundsShape.Size = new Vector2f(bounds.Width, bounds.Height);
            Game.Window.Draw(boundsShape);


        }
    }

    protected override float FuncX(float time) {
        float t = MathF.Max(time - 0.15f, 0f);
        return Velocity * t;
    }

    protected override float FuncY(float time) {
        return 0f;
    }

    protected override Vector2f Adjust(float time, Vector2f position) {
        var screenOffset = new Vector2f(
            MathF.Abs(MathF.Floor(position.X / Game.Window.Size.X)),
            MathF.Abs(MathF.Floor(position.Y / Game.Window.Size.Y))
        );

        return new Vector2f(
            screenOffset.X % 2 * Game.Window.Size.X + TMathF.Mod(position.X, Game.Window.Size.X) * MathF.Pow(-1, screenOffset.X),
            screenOffset.Y % 2 * Game.Window.Size.Y + TMathF.Mod(position.Y, Game.Window.Size.Y) * MathF.Pow(-1, screenOffset.Y)
        );
    }
}