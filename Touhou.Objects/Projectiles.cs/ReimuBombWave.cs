using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Projectiles;

public class BombWave : ParametricProjectile {

    public float Velocity { get; init; }
    public BombWave(Vector2 origin, float direction, bool isP1Owned, bool isPlayerOwned, bool isRemote) : base(origin, direction, isP1Owned, isPlayerOwned, isRemote) { }

    public override void Init() {

        float cos = MathF.Cos(Orientation);
        float sin = MathF.Sin(Orientation);

        float width = (Match.Bounds.Y * MathF.Abs(cos) + Match.Bounds.X * MathF.Abs(sin)) * 2f;

        var offset = new Vector2(cos, sin) * 80f;

        Hitboxes.Add(new RectangleHitbox(this, offset, new Vector2(160f, width), Orientation, IsP1Owned ? CollisionGroup.P1Bomb : CollisionGroup.P2Bomb, Hit));

        base.Init();
    }

    protected override Vector2 PositionFunction(float t) {
        return new Vector2(
            Velocity * t,
            0f
        );
    }

    public override void Render() {

        var alpha = SpawnFactor * Color.A;
        //Log.Info(alpha);


        var sprite = new Sprite("reimubombwave") {
            Origin = new Vector2(0f, 0.5f),
            Position = Position,
            Rotation = Orientation,
            Scale = new Vector2(1f, 20f) * 0.7f,
            Color = new Color4(Color.R, Color.G, Color.B, alpha),
            UseColorSwapping = false,
            UVPaddingOffset = new Vector2(0f, -0.5f),
            BlendMode = BlendMode.Additive,
        };



        Game.Draw(sprite, Layer.PlayerProjectiles);


        var hitbox = new Rectangle() {
            Origin = new Vector2(0.5f),
            Size = Hitboxes[0].GetBounds().Size,
            Position = Hitboxes[0].Position,
            FillColor = Color4.Transparent,
            StrokeWidth = 1f,
            StrokeColor = new Color4(0f, 1f, 0f, 1f),
        };

        base.Render();
    }

    private void Hit(Entity other, Hitbox hitbox) {

        if (!(other is Projectile projectile)) return;

        projectile.NetworkDestroy();

    }
}