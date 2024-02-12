
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class Amulet : ParametricProjectile {

    public float StartingVelocity { get; init; } = 1f;
    public float GoalVelocity { get; init; } = 1f;
    public float VelocityFalloff { get; init; } = 1f;


    private Sprite sprite;

    public Amulet(Vector2 origin, float direction, bool isP1Owned, bool isPlayerOwned, bool isRemote) : base(origin, direction, isP1Owned, isPlayerOwned, isRemote) {

        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), 7.5f, isP1Owned ? CollisionGroup.P1MinorProjectile : CollisionGroup.P2MinorProjectile));

        sprite = new Sprite("amulet") {
            Origin = new Vector2(0.5f, 0.5f),
            Rotation = Orientation,
            UseColorSwapping = true,
        };
    }

    public override void Update() {

        DestroyedOnScreenExit = FuncTimeWithSpawnOffset.AsSeconds() >= 1f ? true : false;

        base.Update();
    }



    protected override Vector2 PositionFunction(float t) {
        var _t = MathF.Min(t, VelocityFalloff);
        return new Vector2(
            (StartingVelocity * MathF.Pow(_t, 2) - GoalVelocity * MathF.Pow(_t, 2) + 2 * GoalVelocity * _t * t - 2 * StartingVelocity * _t * t) / (2 * VelocityFalloff) + StartingVelocity * t,
            0f
        );
    }



    public override void Render() {

        sprite.Position = Position;
        sprite.Scale = new Vector2(0.4f) * (1f + 3f * (1f - SpawnFactor));
        sprite.Color = new Color4(
           Color.R,
           Color.G,
           Color.B,
           Color.A * SpawnFactor * (1f - DestroyedFactor));

        Game.Draw(sprite, IsPlayerOwned ? Layer.PlayerProjectiles : Layer.OpponentProjectiles);
    }
}

