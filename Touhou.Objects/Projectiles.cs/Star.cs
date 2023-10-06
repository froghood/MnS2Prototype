using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Projectiles;

public class ShootingStar : ParametricProjectile {
    private float velocity;
    private float initialRotation;
    private int rotationDirection;

    private TrailStar[] trailStars;

    public ShootingStar(int seed, float velocity, float trailVelocity, Vector2 origin, float orientation, bool isPlayerOwned, bool isRemote) : base(origin, orientation, isPlayerOwned, isRemote) {

        this.velocity = velocity;

        this.initialRotation = Game.Random.NextSingle() * MathF.Tau;
        this.rotationDirection = MathF.Sign(Game.Random.Next(2) - 0.5f);

        this.trailStars = new TrailStar[38];

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 15f, isPlayerOwned ? CollisionGroups.PlayerProjectile : CollisionGroups.OpponentProjectileMinor));

        var random = new Random(seed);

        for (int i = 0; i < trailStars.Length; i++) {
            var randomAngle = random.NextSingle() * MathF.Tau;
            trailStars[i] = new TrailStar(0.15f * (i + 1), randomAngle, velocity, trailVelocity, origin, orientation, isPlayerOwned, isRemote) {
                SpawnDelay = Time.InSeconds(0.25f),
                CanCollide = false,
                Color = new Color4(0f, 1f, 0f, 0.4f),
            };
            trailStars[i].IncreaseTime(TimeOffset, false);
        }


    }

    public override void Init() {
        base.Init();

        foreach (var trailStar in trailStars) {
            Scene.AddEntity(trailStar);
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
            Scale = new Vector2(0.4f) * (1f + 3f * (1f - spawnRatio)),
            Rotation = initialRotation + Game.Time.AsSeconds() * rotationDirection,

            Color = new Color4(
                Color.R,
                Color.G,
                Color.B,
                Color.A * spawnRatio),

            UseColorSwapping = true,

            BlendMode = BlendMode.Additive
        };

        Game.Draw(sprite, IsPlayerOwned ? Layers.PlayerProjectiles1 : Layers.OpponentProjectiles1);
    }

    public override void Receive(Packet packet, IPEndPoint endPoint) {
        if (packet.Type != PacketType.DestroyProjectile) return;

        packet.Out(out uint id).Out(out float theirFuncTime);

        if (Id == id) {
            Destroy();

            for (int i = 0; i < trailStars.Length; i++) {
                var trailStar = trailStars[i];
                if (trailStar.DivergeTime >= theirFuncTime) trailStar.Destroy();
            }


        }


    }

    public override void NetworkDestroy() {

        Destroy();

        for (int i = 0; i < trailStars.Length; i++) {
            var trailStar = trailStars[i];
            if (trailStar.DivergeTime >= FuncTime) trailStar.Destroy();
        }

        var packet = new Packet(PacketType.DestroyProjectile).In(Id ^ 0x80000000).In(FuncTime);
        Game.Network.Send(packet);
    }
}