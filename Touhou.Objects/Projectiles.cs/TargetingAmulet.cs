using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Net;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class TargetingAmulet : ParametricProjectile {
    private readonly float seekingTime = 1.5f;
    private readonly float velocity;
    private readonly float deceleration;



    private Sprite sprite;


    public TargetingAmulet(Vector2 origin, float direction, bool isPlayerOwned, bool isRemote, float velocity, float deceleration, Time spawnTimeOffset = default) : base(origin, direction, isPlayerOwned, isRemote, spawnTimeOffset) {
        this.velocity = velocity;
        this.deceleration = deceleration;

        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), 7.5f, isPlayerOwned ? CollisionGroups.PlayerProjectile : CollisionGroups.OpponentProjectileMinor));

        sprite = new Sprite("amulet") {
            Origin = new Vector2(0.5f, 0.5f),
            Rotation = Direction,
            UseColorSwapping = true,
        };

    }

    protected override float FuncX(float t) {
        return MathF.Max((velocity - t * deceleration) * t, 0f) + MathF.Min((deceleration * t * t) / 2f, MathF.Pow(velocity, 2f) / deceleration / 2f);
    }

    protected override float FuncY(float t) {
        return 0f;
    }

    public override void Render() {
        float spawnTime = MathF.Min(CurrentTime / SpawnDelay.AsSeconds(), 1f);

        sprite.Position = Position;
        sprite.Scale = new Vector2(0.4f, 0.4f) * (1f + 3f * (1f - spawnTime));
        sprite.Color = Color;

        Game.Draw(sprite, IsPlayerOwned ? Layers.PlayerProjectiles1 : Layers.OpponentProjectiles1);

        base.Render();

        // var spriteStates = new SpriteStates() {
        //     Position = Position,
        //     Rotation = TMathF.radToDeg(Direction),
        //     Origin = new Vector2(0.5f, 0.5f),
        //     Scale = new Vector2(0.35f, 0.35f) * (1f + 3f * (1f - spawnTime)),
        // };

        //var Color4 = new Color4(Color4.R, Color4.G, Color4.B, (byte)MathF.Round(Color4.A * spawnTime));

        //var shader = new TShader("projectileColor4");
        //shader.SetUniform("Color4", Color4);

        //Game.DrawSprite("amulet", spriteStates, shader, 0);
    }

    public void LocalTarget(Vector2 targetPosition, Time timeOverflow) {
        Destroy();
        var angle = MathF.Atan2(targetPosition.Y - Position.Y, targetPosition.X - Position.X);

        var projectile = new SpellAmulet(Position, angle, false, false, timeOverflow) {
            GrazeAmount = 1,
            Color = Color,
            StartingVelocity = 500f,
            GoalVelocity = 500f,
        };

        if (Grazed) projectile.Graze();

        Scene.AddEntity(projectile);
    }

    public void RemoteTarget(Time theirTime, Vector2 targetPosition) {
        Destroy();


        var delta = Game.Network.Time - theirTime;
        var angle = MathF.Atan2(targetPosition.Y - Position.Y, targetPosition.X - Position.X);

        var projectile = new SpellAmulet(Position, angle, true, true) {
            InterpolatedOffset = delta.AsSeconds(),
            CanCollide = false,
            Color = Color,
            StartingVelocity = 500f,
            GoalVelocity = 500f,
        };

        Scene.AddEntity(projectile);
    }
}