
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class Amulet : ParametricProjectile {

    public float StartingVelocity { get; init; } = 1f;
    public float GoalVelocity { get; init; } = 1f;
    public float VelocityFalloff { get; init; } = 1f;


    private Sprite sprite;

    public Amulet(Vector2 origin, float direction, bool isPlayerOwned, bool isRemote, Time spawnTimeOffset = default(Time)) : base(origin, direction, isPlayerOwned, isRemote, spawnTimeOffset) {

        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), 7.5f, isPlayerOwned ? CollisionGroups.PlayerProjectile : CollisionGroups.OpponentProjectile));

        sprite = new Sprite("amulet") {
            Origin = new Vector2(0.5f, 0.5f),
            Rotation = Direction,
            UseColorSwapping = true,
        };
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

        sprite.Position = Position;
        sprite.Scale = new Vector2(0.4f, 0.4f) * (1f + 3f * (1f - spawnTime));
        sprite.Color = Color;

        Game.Draw(sprite, IsPlayerOwned ? Layers.PlayerProjectiles1 : Layers.OpponentProjectiles1);

        base.Render();

        // var spriteStates = new SpriteStates() {
        //     Position = Position,
        //     Rotation = TMathF.radToDeg(Direction),
        //     Origin = new Vector2(0.5f, 0.5f),
        //     Scale = new Vector2(0.35f, 0.35f) * (1f + 3f * (1f - spawnTime)),
        // };

        //var Color4 = new Color4(Color4.R, Color4.G, Color4.B, (byte)MathF.Round(Color4.A * spawnTime));

        // var shader = new TShader("projectileColor4");
        // shader.SetUniform("Color4", Color4);

        //Game.DrawSprite("amulet", spriteStates, shader, IsPlayerOwned ? Layers.Projectiles1 : Layers.Projectiles2);

    }

    public override void PostRender() { }

    public override void DebugRender() {
        foreach (CircleHitbox hitbox in Hitboxes) {

        }
    }
}

