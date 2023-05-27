using SFML.Graphics;
using SFML.System;

namespace Touhou.Objects.Projectiles;

public class LinearAmulet : ParametricProjectile {

    public float StartingVelocity { get; init; } = 1f;
    public float GoalVelocity { get; init; } = 1f;
    public float VelocityFalloff { get; init; } = 1f;

    private RectangleShape shape;
    private CircleShape hitboxShape;
    private RectangleShape boundsShape;

    public LinearAmulet(Vector2f origin, float direction, bool isRemote, Time spawnTimeOffset = default(Time)) : base(origin, direction, isRemote, spawnTimeOffset) {

        shape = new RectangleShape(new Vector2f(20f, 15f));
        shape.Origin = shape.Size / 2f;
        shape.Rotation = 180f / MathF.PI * Direction;

        hitboxShape = new CircleShape();
        hitboxShape.FillColor = Color.Transparent;
        hitboxShape.OutlineColor = new Color(0, 255, 0);
        hitboxShape.OutlineThickness = 1f;

        boundsShape = new RectangleShape();
        boundsShape.FillColor = Color.Transparent;
        boundsShape.OutlineColor = new Color(0, 200, 255);
        boundsShape.OutlineThickness = 1f;

        CollisionType = CollisionType.Projectile;
        Hitboxes.Add(new CircleHitbox(this, new Vector2f(0f, 0f), 7.5f));
    }

    protected override float FuncX(float t) {

        var _t = MathF.Min(t, VelocityFalloff);
        return (StartingVelocity * MathF.Pow(_t, 2) - GoalVelocity * MathF.Pow(_t, 2) + 2 * GoalVelocity * _t * t - 2 * StartingVelocity * _t * t) / (2 * VelocityFalloff) + StartingVelocity * t;
    }

    protected override float FuncY(float t) {
        return 0f;
    }


    public override void Render() {
        shape.FillColor = Color;
        shape.Position = Position;
        //float scale = 1f + 2f * MathF.Min(Time - SpawnTime, 0f) / -SpawnTime;
        //byte alpha = (byte)(255 - 255 * MathF.Min(Time - SpawnTime, 0f) / -SpawnTime);
        //_shape.Scale = new Vector2f(scale, scale);
        //_shape.FillColor = new Color(255, 255, 255, alpha);
        Game.Window.Draw(shape);
    }

    public override void PostRender() { }

    public override void DebugRender() {
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
}

