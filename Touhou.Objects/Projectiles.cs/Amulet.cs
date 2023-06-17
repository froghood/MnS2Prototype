using SFML.Graphics;
using SFML.System;

namespace Touhou.Objects.Projectiles;

public class LinearAmulet : ParametricProjectile {

    public float StartingVelocity { get; init; } = 1f;
    public float GoalVelocity { get; init; } = 1f;
    public float VelocityFalloff { get; init; } = 1f;

    public LinearAmulet(Vector2f origin, float direction, bool isRemote, Time spawnTimeOffset = default(Time)) : base(origin, direction, isRemote, spawnTimeOffset) {
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

        float spawnTime = MathF.Min(CurrentTime / SpawnDelay.AsSeconds(), 1f);

        var spriteStates = new SpriteStates() {
            Position = Position,
            Rotation = TMathF.radToDeg(Direction),
            Origin = new Vector2f(0.5f, 0.5f),
            Scale = new Vector2f(0.35f, 0.35f) * (1f + 3f * (1f - spawnTime)),
        };

        var color = new Color(Color.R, Color.G, Color.B, (byte)MathF.Round(Color.A * spawnTime));

        var shader = new TShader("projectileColor");
        shader.SetUniform("color", color);

        Game.DrawSprite("amulet", spriteStates, shader, IsPlayerOwned ? Layers.Projectiles1 : Layers.Projectiles2);

    }

    public override void PostRender() { }

    public override void DebugRender() {
        foreach (CircleHitbox hitbox in Hitboxes) {

        }
    }
}

