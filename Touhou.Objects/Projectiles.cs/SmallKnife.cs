using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class SmallKnife : TimestopProjectile {
    private float velocity;

    public SmallKnife(Vector2 origin, float direction, float velocity, bool isFrozen, bool isPlayerOwned, bool isRemote) : base(origin, direction, isFrozen, isPlayerOwned, isRemote) {
        this.velocity = velocity;

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 4.5f, isPlayerOwned ? CollisionGroups.PlayerProjectile : CollisionGroups.OpponentProjectileMinor));
    }

    protected override Vector2 PositionFunction(float t) {
        return new Vector2(
            velocity * t,
            0f
        );
    }

    public override void Render() {

        float spawnTime = MathF.Min(LifeTime.AsSeconds() / SpawnDelay.AsSeconds(), 1f);


        // timestop saturation
        float middle = (MathF.Max(MathF.Max(Color.R, Color.G), Color.B) + MathF.Min(MathF.Min(Color.R, Color.G), Color.B)) / 2f;
        float saturation = IsTimestopped ? 0.25f : 1f;


        // invert Y scale depending on if it's facing right or left 
        var flipped = new Vector2(1f, MathF.Abs(Tangent) > MathF.PI / 2f ? -1f : 1f);


        var sprite = new Sprite("smallknife") {

            Origin = new Vector2(0.5f),

            Position = Position,
            Rotation = Tangent,
            Scale = new Vector2(0.35f) * flipped * (1f + 3f * (1f - spawnTime)),



            Color = new Color4(
                middle + (Color.R - middle) * saturation,
                middle + (Color.G - middle) * saturation,
                middle + (Color.B - middle) * saturation,
                Color.A * spawnTime),

            UseColorSwapping = true,
        };

        Game.Draw(sprite, IsPlayerOwned ? Layers.PlayerProjectiles1 : Layers.OpponentProjectiles1);

        base.Render();
    }
}