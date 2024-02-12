using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Projectiles;

public class AuxiliaryStar : ParametricProjectile {

    public float DivergeTime { get => divergeTime; }

    private uint parentId;
    private float size;
    private float divergeTime;
    private float divergeAngle;
    private float beforeDivergeVelocity;
    private float afterDivergeVelocity;



    private float initialRotation;
    private int rotationDirection;



    public AuxiliaryStar(uint parentId, float size, float divergeTime, float divergeAngle, float beforeDivergeVelocity, float afterDivergeVelocity, Vector2 origin, float orientation, bool isP1Owned, bool isPlayerOwned, bool isRemote) : base(origin, orientation, isP1Owned, isPlayerOwned, isRemote) {

        this.parentId = parentId;
        this.size = size;
        this.divergeTime = divergeTime;
        this.divergeAngle = divergeAngle;
        this.beforeDivergeVelocity = beforeDivergeVelocity;
        this.afterDivergeVelocity = afterDivergeVelocity;

        this.initialRotation = Game.Random.NextSingle() * MathF.Tau;
        this.rotationDirection = MathF.Sign(Game.Random.Next(2) - 0.5f);

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, size, isP1Owned ? CollisionGroup.P1MinorProjectile : CollisionGroup.P2MinorProjectile));
    }

    protected override Vector2 PositionFunction(float t) {
        return new Vector2(
            MathF.Min(t, divergeTime) * beforeDivergeVelocity + MathF.Max(t - divergeTime, 0f) * MathF.Cos(divergeAngle) * afterDivergeVelocity,
            MathF.Max(t - divergeTime, 0f) * MathF.Sin(divergeAngle) * afterDivergeVelocity
        );
    }

    public override void Update() {

        DestroyedOnScreenExit = FuncTimeWithSpawnOffset.AsSeconds() >= divergeTime + 1f ? true : false;
        CanCollide = !IsP1Owned && (FuncTimeWithSpawnOffset.AsSeconds() >= divergeTime ? true : false);

        base.Update();
    }

    public override void Render() {

        if (FuncTimeWithSpawnOffset.AsSeconds() < divergeTime) return;

        float divergeRatio = Math.Clamp(FuncTimeWithSpawnOffset.AsSeconds() - divergeTime, 0f, 0.25f) * 4f;

        var sprite = new Sprite("star") {

            Origin = new Vector2(0.5f),

            Position = Position,
            Scale = new Vector2(size / 40f),
            Rotation = initialRotation + Game.Time.AsSeconds() * size / 8f * rotationDirection,

            Color = new Color4(
                Color.R,
                Color.G,
                Color.B,
                Color.A * divergeRatio * (1f - DestroyedFactor)),

            UseColorSwapping = true,

            BlendMode = BlendMode.Additive
        };

        Game.Draw(sprite, IsPlayerOwned ? Layer.PlayerProjectiles : Layer.OpponentProjectiles);
    }

    public override void Receive(Packet packet, IPEndPoint endPoint) {

        if (packet.Type != PacketType.DestroyProjectile) return;

        packet.Out(out uint id, true);

        if (id == Id) Destroy();

        // must retroactively destroy self in the case where the parent has already been destroyed by other means

        if (id == parentId) {
            packet.Out(out float theirFuncTime);

            if (divergeTime > theirFuncTime) Destroy();
        }
    }
}