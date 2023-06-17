using System.Net;
using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class TargetingAmulet : ParametricProjectile {
    private readonly float seekingTime = 1.5f;
    private readonly float velocity;
    private readonly float deceleration;


    private readonly RectangleShape shape;

    public TargetingAmulet(Vector2f origin, float direction, bool isRemote, float velocity, float deceleration, Time spawnTimeOffset = default) : base(origin, direction, isRemote, spawnTimeOffset) {
        this.velocity = velocity;
        this.deceleration = deceleration;

        shape = new RectangleShape(new Vector2f(20f, 15f));
        shape.Origin = shape.Size / 2f;
        shape.Rotation = 180f / MathF.PI * Direction;
        shape.FillColor = Color;

        CollisionType = CollisionType.Projectile;
        Hitboxes.Add(new CircleHitbox(this, new Vector2f(0f, 0f), 7.5f));

    }

    protected override float FuncX(float t) {
        return MathF.Max((velocity - t * deceleration) * t, 0f) + MathF.Min((deceleration * t * t) / 2f, MathF.Pow(velocity, 2f) / deceleration / 2f);
    }

    protected override float FuncY(float t) {
        return 0f;
    }

    public override void Render() {
        float spawnTime = MathF.Min(CurrentTime / SpawnDelay.AsSeconds(), 1f);

        var spriteStates = new SpriteStates() {
            Position = Position,
            Rotation = TMathF.radToDeg(Direction),
            Origin = new Vector2f(0.5f, 0.5f),
            Scale = new Vector2f(0.35f, 0.35f) * (1f + 3f * (1f - spawnTime)),
        };

        var color = new Color(Color.R, Color.G, Color.B, (byte)MathF.Round(Color.A * spawnTime));

        var shader = new TShader("projectileColor");
        shader.SetUniform("color", color);

        Game.DrawSprite("amulet", spriteStates, shader, 0);
    }

    public void LocalTarget(Vector2f targetPosition, Time timeOverflow) {
        Destroy();
        var angle = MathF.Atan2(targetPosition.Y - Position.Y, targetPosition.X - Position.X);

        var projectile = new LinearAmulet(Position, angle, false, timeOverflow) {
            GrazeAmount = 1,
            Color = Color,
            StartingVelocity = 500f,
            GoalVelocity = 500f,
        };
        projectile.CollisionGroups.Add(1);
        if (Grazed) projectile.Graze();

        Scene.AddEntity(projectile);
    }

    public void RemoteTarget(Time theirTime, Vector2f targetPosition) {
        Destroy();


        var delta = Game.Network.Time - theirTime;
        var angle = MathF.Atan2(targetPosition.Y - Position.Y, targetPosition.X - Position.X);

        var projectile = new LinearAmulet(Position, angle, true) {
            InterpolatedOffset = delta.AsSeconds(),
            CanCollide = false,
            Color = Color,
            StartingVelocity = 500f,
            GoalVelocity = 500f,
        };

        Scene.AddEntity(projectile);
    }
}