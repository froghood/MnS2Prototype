using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Projectiles;

public class ShootingStar : ParametricProjectile {


    public int TrailGrazeAmount { get; init; }


    private readonly int seed;
    private readonly float velocity;
    private readonly float trailVelocity;


    private readonly float initialRotation;
    private readonly int rotationDirection;

    private readonly AuxiliaryStar[] trailStars;

    public ShootingStar(int seed, float velocity, float trailVelocity, Vector2 origin, float orientation, bool isP1Owned, bool isPlayerOwned, bool isRemote) : base(origin, orientation, isP1Owned, isPlayerOwned, isRemote) {

        this.seed = seed;
        this.velocity = velocity;
        this.trailVelocity = trailVelocity;

        this.initialRotation = Game.Random.NextSingle() * MathF.Tau;
        this.rotationDirection = MathF.Sign(Game.Random.Next(2) - 0.5f);

        this.trailStars = new AuxiliaryStar[38];

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 20f, isP1Owned ? CollisionGroup.P1MinorProjectile : CollisionGroup.P2MinorProjectile));

    }

    public override void Init() {
        base.Init();

        var random = new Random(seed);

        for (int i = 0; i < trailStars.Length; i++) {
            var randomAngle = random.NextSingle() * MathF.Tau;
            var randomOffsetAngle = random.NextSingle() * MathF.Tau;

            var offset = new Vector2(MathF.Cos(randomOffsetAngle), MathF.Sin(randomOffsetAngle)) * 10f;

            trailStars[i] = new AuxiliaryStar(Id, 8f, 0.1f * i + 0.05f, randomAngle, velocity, trailVelocity, Origin + offset, Orientation, IsP1Owned, IsPlayerOwned, IsRemote) {
                SpawnDuration = SpawnDuration,
                Color = Color,
                GrazeAmount = TrailGrazeAmount,
            };

            Scene.AddEntity(trailStars[i]);
        }
    }

    protected override Vector2 PositionFunction(float t) {
        return new Vector2(velocity * t, 0f);
    }

    public override void Render() {

        float spawnRatio = MathF.Min(LifeTime.AsSeconds() / SpawnDelay.AsSeconds(), 1f);

        var sprite = new Sprite("star") {

            Origin = new Vector2(0.5f),

            Position = Position,
            Scale = new Vector2(0.45f) * (1f + 3f * (1f - spawnRatio)),
            Rotation = initialRotation + Game.Time.AsSeconds() * 3f * rotationDirection,

            Color = new Color4(
                Color.R,
                Color.G,
                Color.B,
                Color.A * spawnRatio * (1f - DestroyedFactor)),

            UseColorSwapping = true,

            BlendMode = BlendMode.Additive
        };

        Game.Draw(sprite, IsPlayerOwned ? Layer.PlayerProjectiles : Layer.OpponentProjectiles);
    }

    public override void Receive(Packet packet) {
        if (packet.Type != PacketType.DestroyProjectile) return;

        packet.Out(out uint id, true).Out(out float theirFuncTime);

        if (Id == id) {
            base.Destroy();

            foreach (var star in trailStars) {
                if (star.DivergeTime > theirFuncTime) star.Destroy();
            }
        }
    }

    public override void ForwardTime(Time amount, bool interpolate) {

        base.ForwardTime(amount, interpolate);

        foreach (var trailStar in trailStars) {
            trailStar.ForwardTime(amount, interpolate);
        }
    }



    public override void Destroy() {
        base.Destroy();

        foreach (var star in trailStars) {
            if (star.DivergeTime > FuncTimeWithSpawnOffset.AsSeconds()) star.Destroy();
        }
    }

    public override void Destroy(Time delay) {
        base.Destroy(delay);

        foreach (var star in trailStars) {
            if (star.DivergeTime >= FuncTimeWithSpawnOffset.AsSeconds()) star.Destroy();
        }
    }

    public override void NetworkDestroy() {
        Destroy();

        Game.NetworkOld.Send(
            PacketType.DestroyProjectile,
            Id ^ 0x80000000,
            FuncTimeWithSpawnOffset.AsSeconds());

    }
}