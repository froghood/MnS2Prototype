using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class TargetingAmulet : ParametricProjectile {
    private readonly float seekingTime = 1.5f;
    private readonly float velocity;
    private readonly float deceleration;



    private Sprite sprite;


    public TargetingAmulet(Vector2 origin, float direction, bool isP1Owned, bool isPlayerOwned, bool isRemote, float velocity, float deceleration) : base(origin, direction, isP1Owned, isPlayerOwned, isRemote) {
        this.velocity = velocity;
        this.deceleration = deceleration;

        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), 7.5f, isP1Owned ? CollisionGroup.P1MinorProjectile : CollisionGroup.P2MinorProjectile));

        sprite = new Sprite("amulet") {
            Origin = new Vector2(0.5f, 0.5f),
            Rotation = Orientation,
            UseColorSwapping = true,
        };

    }

    protected override Vector2 PositionFunction(float t) {
        return new Vector2(
            MathF.Max((velocity - t * deceleration) * t, 0f) +
            MathF.Min((deceleration * t * t) / 2f, MathF.Pow(velocity, 2f) / deceleration / 2f),
            0f
        );
    }

    public override void Render() {
        float spawnRatio = MathF.Min(LifeTime.AsSeconds() / SpawnDelay.AsSeconds(), 1f);

        sprite.Position = Position;
        sprite.Scale = new Vector2(0.4f, 0.4f) * (1f + 3f * (1f - spawnRatio));
        sprite.Color = new Color4(
            Color.R,
            Color.G,
            Color.B,
            Color.A * spawnRatio);

        Game.Draw(sprite, IsPlayerOwned ? Layer.PlayerProjectiles : Layer.OpponentProjectiles);

    }

    public void LocalTarget(Vector2 targetPosition, Time timeOverflow) {
        Destroy();
        var angle = MathF.Atan2(targetPosition.Y - Position.Y, targetPosition.X - Position.X);

        var projectile = new SpecialAmulet(Position, angle, IsP1Owned, IsPlayerOwned, false) {
            GrazeAmount = 1,
            Color = Color,
            StartingVelocity = 500f,
            GoalVelocity = 500f,
        };
        projectile.ForwardTime(timeOverflow, false);

        if (Grazed) projectile.Graze();

        Scene.AddEntity(projectile);
    }

    public void RemoteTarget(Time theirTime, Vector2 targetPosition) {
        Destroy();


        var latency = Game.Network.Time - theirTime;
        var angle = MathF.Atan2(targetPosition.Y - Position.Y, targetPosition.X - Position.X);

        var projectile = new SpecialAmulet(Position, angle, IsP1Owned, IsPlayerOwned, true) {
            CanCollide = false,
            Color = Color,
            StartingVelocity = 500f,
            GoalVelocity = 500f,
        };
        projectile.ForwardTime(latency, true);

        Scene.AddEntity(projectile);
    }
}