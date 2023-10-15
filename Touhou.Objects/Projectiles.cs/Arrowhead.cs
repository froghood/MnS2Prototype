using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class Arrowhead : ParametricProjectile {
    private readonly float velocity;

    public Arrowhead(Vector2 origin, float orientation, float velocity, bool isPlayerOwned, bool isRemote) : base(origin, orientation, isPlayerOwned, isRemote) {
        this.velocity = velocity;

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 10f, isPlayerOwned ? CollisionGroup.PlayerProjectile : CollisionGroup.OpponentProjectileMinor));
    }



    protected override Vector2 PositionFunction(float t) {
        return new Vector2(velocity * t, 0f);
    }

    public override void Render() {

        float spawnRatio = MathF.Min(LifeTime.AsSeconds() / SpawnDelay.AsSeconds(), 1f);

        var sprite = new Sprite("arrowhead") {
            Origin = new Vector2(0.5f),
            Position = Position,
            Rotation = Orientation,
            Scale = new Vector2(0.45f) * (1f + 3f * (1f - spawnRatio)),
            Color = new Color4(Color.R, Color.G, Color.B, Color.A * spawnRatio),
            UseColorSwapping = true,
        };

        Game.Draw(sprite, IsPlayerOwned ? Layer.PlayerProjectiles1 : Layer.OpponentProjectiles1);
    }
}