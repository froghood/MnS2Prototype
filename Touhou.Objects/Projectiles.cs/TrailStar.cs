using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class TrailStar : ParametricProjectile {

    public float DivergeTime { get => divergeTime; }
    private float divergeTime;
    private float divergeAngle;
    private float beforeDivergeVelocity;
    private float afterDivergeVelocity;



    private float initialRotation;
    private int rotationDirection;



    public TrailStar(float divergeTime, float divergeAngle, float beforeDivergeVelocity, float afterDivergeVelocity, Vector2 origin, float orientation, bool isPlayerOwned, bool isRemote) : base(origin, orientation, isPlayerOwned, isRemote) {

        this.divergeTime = divergeTime;
        this.divergeAngle = divergeAngle;
        this.beforeDivergeVelocity = beforeDivergeVelocity;
        this.afterDivergeVelocity = afterDivergeVelocity;

        this.initialRotation = Game.Random.NextSingle() * MathF.Tau;
        this.rotationDirection = MathF.Sign(Game.Random.Next(2) - 0.5f);

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 5f, isPlayerOwned ? CollisionGroups.PlayerProjectile : CollisionGroups.OpponentProjectileMinor));

    }

    protected override Vector2 PositionFunction(float t) {
        return new Vector2(
            MathF.Min(t, divergeTime) * beforeDivergeVelocity + MathF.Max(t - divergeTime, 0f) * MathF.Cos(divergeAngle) * afterDivergeVelocity,
            MathF.Max(t - divergeTime, 0f) * MathF.Sin(divergeAngle) * afterDivergeVelocity
        );
    }

    public override void Update() {

        CanCollide = FuncTime >= divergeTime ? true : false;

        base.Update();
    }

    public override void Render() {

        if (FuncTime < divergeTime) return;

        float divergeRatio = Math.Clamp(FuncTime - divergeTime, 0f, 0.25f) * 4f;

        var sprite = new Sprite("star") {

            Origin = new Vector2(0.5f),

            Position = Position,
            Scale = new Vector2(0.2f),
            Rotation = initialRotation + Game.Time.AsSeconds() * rotationDirection,

            Color = new Color4(
                Color.R,
                Color.G,
                Color.B,
                Color.A * divergeRatio),

            UseColorSwapping = true,

            BlendMode = BlendMode.Additive
        };

        Game.Draw(sprite, IsPlayerOwned ? Layers.PlayerProjectiles1 : Layers.OpponentProjectiles1);
    }
}