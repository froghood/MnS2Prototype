using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class SmallKnife : TimestopProjectile {
    private float velocity;

    public SmallKnife(Vector2 origin, float direction, float velocity, bool isFrozen, bool isPlayerOwned, bool isRemote) : base(origin, direction, isFrozen, isPlayerOwned, isRemote) {
        this.velocity = velocity;

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 3f, isPlayerOwned ? CollisionGroups.PlayerProjectile : CollisionGroups.OpponentProjectileMinor));
    }

    protected override Vector2 PositionFunction(float t) {
        return new Vector2(
            velocity * t,
            0f
        );
    }

    public override void Render() {

        float spawnTime = MathF.Min(LifeTime.AsSeconds() / SpawnDelay.AsSeconds(), 1f);

        var sprite = new Sprite("knife2") {

            Origin = new Vector2(0.5f),

            Position = Position,
            Rotation = Tangent,
            Scale = new Vector2(0.35f) * (1f + 3f * (1f - spawnTime)),

            Color = new Color4(
                Color.R,
                Color.G,
                Color.B,
                Color.A * spawnTime),

            UseColorSwapping = true,
        };

        Game.Draw(sprite, IsPlayerOwned ? Layers.PlayerProjectiles1 : Layers.OpponentProjectiles1);

        // Game.Draw(new Circle {
        //     Origin = new Vector2(0.5f),
        //     Position = Position,
        //     Radius = ((CircleHitbox)Hitboxes[0]).Radius,
        //     StrokeWidth = 1f,
        //     StrokeColor = new Color4(1f, 1f, 1f, 1f),
        //     FillColor = Color4.Transparent,

        // }, Layers.Foreground2);

        base.Render();
    }
}