using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Projectiles;

public class ReimuBombWave : ParametricProjectile {

    public float Velocity { get; init; }
    public ReimuBombWave(Vector2 origin, float direction, bool isPlayerOwned, bool isRemote, Time spawnTimeOffset = default) : base(origin, direction, isPlayerOwned, isRemote, spawnTimeOffset) { }

    public override void Init() {

        float width = (Match.Bounds.Y * MathF.Abs(MathF.Cos(Direction)) + Match.Bounds.X * MathF.Abs(MathF.Sin(Direction))) * 2f;

        Hitboxes.Add(new RectangleHitbox(this, Vector2.Zero, new Vector2(250f, width), Direction, IsPlayerOwned ? CollisionGroups.PlayerBomb : CollisionGroups.OpponentBomb, Hit));

        base.Init();
    }

    protected override float FuncX(float t) {
        return t * Velocity;
    }

    protected override float FuncY(float t) {
        return 0f;
    }

    public override void Render() {

        var alpha = MathF.Min(SpawnDelay.AsSeconds(), CurrentTime) / SpawnDelay.AsSeconds() * Color.A;
        //System.Console.WriteLine(alpha);


        var sprite = new Sprite("reimubombwave") {
            Origin = new Vector2(0f, 0.5f),
            Position = Position,
            Rotation = Direction,
            Scale = new Vector2(1f, 20f) * 0.7f,
            Color = new Color4(Color.R, Color.G, Color.B, alpha),
            UseColorSwapping = false,
            BlendMode = BlendMode.Additive,
        };



        Game.Draw(sprite, Layers.PlayerProjectiles1);


        var hitbox = new Rectangle() {
            Origin = new Vector2(0.5f),
            Size = Hitboxes[0].GetBounds().Size,
            Position = Hitboxes[0].Position,
            FillColor = Color4.Transparent,
            StrokeWidth = 1f,
            StrokeColor = new Color4(0f, 1f, 0f, 1f),
        };

        //Game.Draw(hitbox, Layers.Foreground2);


        base.Render();
    }

    private void Hit(Entity other) {

        if (!(other is Projectile projectile)) return;

        projectile.Destroy();

        // must toggle the last bit because the opponents' projectile ids are opposite
        var packet = new Packet(PacketType.DestroyProjectile).In(projectile.Id ^ 0x80000000);
        Game.Network.Send(packet);

    }
}