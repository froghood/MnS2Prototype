using System.Net;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Objects.Projectiles;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Characters;
public abstract class Opponent : Entity, IReceivable {


    private Vector2f basePosition;
    private Vector2f predictedOffset;
    private Vector2f interpolatedPosition;
    private float predictedOffsetInterpolationTime;
    private Vector2f velocity;
    private Vector2f smoothingOffset;
    private float smoothingOffsetInterpolationTime;



    private bool isHit;
    private Time knockbackTime;
    private Vector2f knockbackStartPosition;
    private Vector2f knockbackEndPosition;
    private Time knockbackDuration;
    private bool isDead;



    private Dictionary<PacketType, Attack> attacks = new();
    private bool matchStarted;

    public Color Color { get; set; } = new Color(255, 0, 100);

    public Opponent(Vector2f startingPosition) {
        basePosition = startingPosition;
    }

    public virtual void Receive(Packet packet, IPEndPoint endPoint) {

        if (packet.Type == PacketType.MatchStart) {
            matchStarted = true;
        }

        if (!matchStarted) return;


        if (packet.Type == PacketType.VelocityChange) {
            packet.Out(out Time theirTime, true).Out(out Vector2f theirPosition).Out(out Vector2f theirVelocity);

            var latency = Game.Network.Time - theirTime;


            System.Console.WriteLine(smoothingOffset);

            basePosition = theirPosition;
            smoothingOffset = Position - basePosition;


            Position = basePosition + smoothingOffset;

            velocity = theirVelocity;

            predictedOffset = velocity * latency.AsSeconds();

            predictedOffsetInterpolationTime = 0f;
            smoothingOffsetInterpolationTime = 1f;

            isHit = false;
        } else if (packet.Type == PacketType.Hit) {
            packet.Out(out Time theirTime, true).Out(out Vector2f theirPosition).Out(out float angle);

            Scene.AddEntity(new HitExplosion(theirPosition, 0.5f, 100f, Color));

            var latency = Game.Network.Time - theirTime;

            isHit = true;
            knockbackTime = Game.Time;
            knockbackStartPosition = theirPosition;
            knockbackEndPosition = theirPosition + new Vector2f(MathF.Cos(angle), MathF.Sin(angle)) * 100f;
            knockbackDuration = Time.InSeconds(1);

            Game.Sounds.Play("hit");

        } else if (packet.Type == PacketType.Death) {
            isDead = true;
            packet.Out(out Time _, true).Out(out Vector2f theirPosition);

            Scene.AddEntity(new HitExplosion(theirPosition, 1f, 500f, Color));

            Game.Sounds.Play("death");
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
            Position = basePosition +
                predictedOffset * Easing.InOut(predictedOffsetInterpolationTime, 3f) +
                smoothingOffset * Easing.In(smoothingOffsetInterpolationTime, 8f);
        }

        Position = new Vector2f() {
            X = Math.Clamp(Position.X, 5f, Game.Window.Size.X - 5f),
            Y = Math.Clamp(Position.Y, 5f, Game.Window.Size.Y - 5f)
        };
    }

    private void UpdateKnockback() {
        var t = MathF.Min((Game.Time - knockbackTime).AsSeconds() / knockbackDuration.AsSeconds(), 1f);

        Position = (knockbackEndPosition - knockbackStartPosition) * Easing.Out(t, 5f) + knockbackStartPosition;
        //if (t == 1f) isHit = false;
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
        circle.Position = predictedOffset;
        Game.Window.Draw(circle);

        circle.FillColor = new Color(0, 255, 0);
        circle.Position = interpolatedPosition;
        Game.Window.Draw(circle);


    }

    public override void PostRender() {
        predictedOffsetInterpolationTime = MathF.Min(predictedOffsetInterpolationTime + Game.Delta.AsSeconds() * 0.5f, 1f);
        smoothingOffsetInterpolationTime = MathF.Max(smoothingOffsetInterpolationTime - Game.Delta.AsSeconds(), 0f);

    }

    protected void AddAttack(PacketType type, Attack attack) => attacks[type] = attack;
}