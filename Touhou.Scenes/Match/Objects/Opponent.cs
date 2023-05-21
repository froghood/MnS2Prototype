using System.Net;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Scenes.Match.Objects.Characters;

namespace Touhou.Scenes.Match.Objects;
public abstract class Opponent : Entity, IReceivable {

    private Vector2f basePosition;
    private Vector2f predictedPosition;
    private Vector2f interpolatedPosition;
    private float interpolationTime;
    private Vector2f velocity;

    private bool isHit;
    private Time knockbackTime;
    private Vector2f knockbackStartPosition;
    private Vector2f knockbackEndPosition;
    private Time knockbackDuration;

    private Dictionary<PacketType, Attack> attacks = new();
    private bool isDead;

    // projectile tracking
    private Dictionary<uint, Projectile> projectiles = new();
    private uint totalSpawnedProjectiles;

    public Color Color { get; set; } = new Color(255, 0, 100);

    public Opponent(Vector2f startingPosition) {
        basePosition = startingPosition;
        predictedPosition = startingPosition;
        Position = Position;
    }

    public virtual void Receive(Packet packet, IPEndPoint endPoint) {


        if (packet.Type == PacketType.VelocityChange) {
            packet.Out(out Time theirTime, true).Out(out Vector2f theirPosition).Out(out Vector2f theirVelocity);

            var latency = Game.Network.Time - theirTime;

            velocity = theirVelocity;
            basePosition = theirPosition;

            predictedPosition = basePosition + velocity * latency.AsSeconds();

            interpolationTime = 0f;

            isHit = false;
        } else if (packet.Type == PacketType.Hit) {
            packet.Out(out Time theirTime, true).Out(out Vector2f theirPosition).Out(out float angle);

            Scene.AddEntity(new HitExplosion(theirPosition, 0.5f, 100f, Color));

            var latency = Game.Network.Time - theirTime;

            isHit = true;
            knockbackTime = Game.Time - latency;
            knockbackStartPosition = theirPosition;
            knockbackEndPosition = theirPosition + new Vector2f(100f * MathF.Cos(angle), 100f * MathF.Sin(angle));
            knockbackDuration = Time.InSeconds(1);

            Game.Sounds.Play("se_pldead00");

        } else if (packet.Type == PacketType.Death) {
            isDead = true;
            packet.Out(out Time _, true).Out(out Vector2f theirPosition);

            Scene.AddEntity(new HitExplosion(theirPosition, 1f, 500f, Color));

            Game.Sounds.Play("se_enep01");
        }

        if (attacks.TryGetValue(packet.Type, out var attack)) {
            packet.ResetReadPosition();
            attack.OpponentPress(this, packet);
        }




        packet.ResetReadPosition();
    }

    public override void Update() {

        if (isHit) {
            UpdateKnockback();
        } else {
            basePosition += velocity * Game.Delta.AsSeconds();
            predictedPosition += velocity * Game.Delta.AsSeconds();
            Position = basePosition + (predictedPosition - basePosition) * EaseInOutCubic(interpolationTime);
        }

        Position = new Vector2f() {
            X = Math.Clamp(Position.X, 5f, Game.Window.Size.X - 5f),
            Y = Math.Clamp(Position.Y, 5f, Game.Window.Size.Y - 5f)
        };
    }

    private void UpdateKnockback() {
        var t = MathF.Min((Game.Time - knockbackTime) / (float)knockbackDuration, 1f);
        var easing = 1f - MathF.Pow(1f - t, 5f);

        Position = (knockbackEndPosition - knockbackStartPosition) * easing + knockbackStartPosition;
    }

    public override void Render() {
        if (isDead) return;
        var rect = new RectangleShape(new Vector2f(20f, 20f));
        rect.Origin = rect.Size / 2f;
        rect.Position = Position;
        rect.FillColor = Color;
        Game.Window.Draw(rect);
    }

    public override void DebugRender() {
        var circle = new CircleShape(5);
        circle.Origin = new Vector2f(circle.Radius, circle.Radius);

        circle.FillColor = new Color(0, 200, 255);
        circle.Position = basePosition;
        Game.Window.Draw(circle);

        circle.FillColor = new Color(255, 100, 0);
        circle.Position = predictedPosition;
        Game.Window.Draw(circle);

        circle.FillColor = new Color(0, 255, 0);
        circle.Position = interpolatedPosition;
        Game.Window.Draw(circle);
    }

    public override void PostRender() {
        interpolationTime = MathF.Min(interpolationTime + Game.Delta.AsSeconds() * 0.5f, 1f);
    }

    public void SpawnProjectile(Projectile projectile) {
        // flips the last bit to a 1 to signify the projectile being the opponents
        projectile.SetId(totalSpawnedProjectiles ^ 0x80000000);

        projectile.Destroyed += () => projectiles.Remove(projectile.Id);
        projectiles.Add(projectile.Id, projectile);
        Scene.AddEntity(projectile);

        totalSpawnedProjectiles++;
    }

    protected void AddAttack(PacketType type, Attack attack) => attacks[type] = attack;

    private float EaseInOutCubic(float t) {
        return t < 0.5 ? 4 * t * t * t : 1 - MathF.Pow(-2 * t + 2, 3) / 2;
    }
}