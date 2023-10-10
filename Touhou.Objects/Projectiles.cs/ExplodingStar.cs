using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Projectiles;

public class ExplodingStar : ParametricProjectile {

    public int ExplosionGrazeAmount { get; init; }
    private readonly float velocity;
    private readonly float explosionVelocity;
    private readonly AuxiliaryStar[] explosionStars;

    public ExplodingStar(float velocity, float explosionVelocity, Vector2 origin, float orientation, bool isPlayerOwned, bool isRemote) : base(origin, orientation, isPlayerOwned, isRemote) {
        this.velocity = velocity;
        this.explosionVelocity = explosionVelocity;

        this.explosionStars = new AuxiliaryStar[28];

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 30f, isPlayerOwned ? CollisionGroup.PlayerProjectile : CollisionGroup.OpponentProjectileMinor));
    }

    public override void Init() {
        base.Init();

        for (int i = 0; i < explosionStars.Length; i++) {
            var divergeAngle = MathF.Tau / explosionStars.Length * i;

            var modifiedVelocity = explosionVelocity * (1f + (i + 1) % 2 * 0.4f);
            var offset = new Vector2(MathF.Cos(Orientation + divergeAngle), MathF.Sin(Orientation + divergeAngle)) * 20f;

            explosionStars[i] = new AuxiliaryStar(Id, 10f, 1f, divergeAngle, velocity, modifiedVelocity, Origin + offset, Orientation, IsPlayerOwned, IsRemote) {
                SpawnDelay = SpawnDelay,
                Color = Color,
                GrazeAmount = ExplosionGrazeAmount
            };

            Scene.AddEntity(explosionStars[i]);
        }
    }

    protected override Vector2 PositionFunction(float t) {

        t = 1 - MathF.Pow(MathF.Max(1 - t, 0f), 2f);

        return new Vector2(velocity * t, 0f);
    }

    public override void Update() {

        if (FuncTimeWithSpawnDelay >= Time.InSeconds(1f)) Destroy();

        base.Update();
    }

    public override void Render() {

        float spawnRatio = MathF.Min(LifeTime.AsSeconds() / SpawnDelay.AsSeconds(), 1f);

        var sprite = new Sprite("vortex") {

            Origin = new Vector2(0.5f),

            Position = Position,
            Scale = new Vector2(0.45f) * (1f + 3f * (1f - spawnRatio)),
            Rotation = -Game.Time.AsSeconds() * 4f,

            Color = new Color4(
                Color.R,
                Color.G,
                Color.B,
                Color.A * spawnRatio),

            UseColorSwapping = true,

            BlendMode = BlendMode.Additive
        };

        Game.Draw(sprite, IsPlayerOwned ? Layer.PlayerProjectiles1 : Layer.OpponentProjectiles1);
    }

    public override void Receive(Packet packet, IPEndPoint endPoint) {
        if (packet.Type != PacketType.DestroyProjectile) return;

        packet.Out(out uint id, true).Out(out float theirFuncTime);

        if (Id == id) {
            base.Destroy();

            foreach (var star in explosionStars) {
                if (star.DivergeTime > theirFuncTime) star.Destroy();
            }
        }
    }

    public override void ForwardTime(Time amount, bool interpolate) {
        base.ForwardTime(amount, interpolate);

        foreach (var star in explosionStars) {
            star.ForwardTime(amount, interpolate);
        }
    }

    public override void Destroy() {
        base.Destroy();

        foreach (var star in explosionStars) {
            if (star.DivergeTime > FuncTimeWithSpawnDelay.AsSeconds()) star.Destroy();
        }
    }

    public override void NetworkDestroy() {
        Destroy();

        var packet = new Packet(PacketType.DestroyProjectile).In(Id ^ 0x80000000).In(FuncTimeWithSpawnDelay.AsSeconds());
        Game.Network.Send(packet);
    }
}