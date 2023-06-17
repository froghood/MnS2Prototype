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


    private Dictionary<PacketType, Action<Packet>> packetDelegates;



    public Dictionary<PlayerAction, Attack> Attacks { get; } = new();
    private Dictionary<Attack, (Time Time, bool Focused)> currentlyHeldAttacks = new();


    private bool matchStarted;

    public Color Color { get; set; } = new Color(255, 0, 100);

    private Player Player => player is null ? player = Scene.GetFirstEntity<Player>() : player;
    private Player player;

    private Match Match => match is null ? match = Scene.GetFirstEntity<Match>() : match;
    private Match match;



    // power
    public int Power { get => Math.Min(Match.TotalPowerGenerated + powerGainedFromGrazing - powerSpent, 400); }
    private int powerGainedFromGrazing;
    private int powerSpent;



    public int HeartCount { get; private set; } = 5;



    public Opponent(Vector2f startingPosition) {
        basePosition = startingPosition;

        packetDelegates = new Dictionary<PacketType, Action<Packet>>() {
            {PacketType.MatchStarted, (_) => {System.Console.WriteLine("match started");matchStarted = true;}},
            {PacketType.VelocityChanged, VelocityChanged},
            {PacketType.AttackPressed, AttackPressed},
            {PacketType.AttackReleased, AttackReleased},
            {PacketType.SpentPower, SpentPower},
            {PacketType.Grazed, Grazed},
            {PacketType.Hit, Hit},
            {PacketType.Death, Death},
        };
    }



    public virtual void Receive(Packet packet, IPEndPoint endPoint) {

        if (packet.Type != PacketType.MatchStarted && !matchStarted) return;

        if (packetDelegates.TryGetValue(packet.Type, out var action)) {
            action.Invoke(packet);
        }
    }



    public override void Update() {

        if (isHit) {
            UpdateKnockback();
        } else {
            basePosition += velocity * Game.Delta.AsSeconds();
            Position = basePosition +
                predictedOffset * Easing.InOut(predictedOffsetInterpolationTime, 3f) +
                smoothingOffset * Easing.In(smoothingOffsetInterpolationTime, 12f);
        }

        Position = new Vector2f(
            Math.Clamp(Position.X, -Match.Bounds.X, Match.Bounds.X),
            Math.Clamp(Position.Y, -Match.Bounds.Y, Match.Bounds.Y)
        );
    }

    private void UpdateKnockback() {
        var t = MathF.Min((Game.Time - knockbackTime).AsSeconds() / knockbackDuration.AsSeconds(), 1f);

        Position = (knockbackEndPosition - knockbackStartPosition) * Easing.Out(t, 5f) + knockbackStartPosition;
        //if (t == 1f) isHit = false;
    }

    public override void Render() {
        if (isDead) return;

        var states = new SpriteStates() {
            Origin = new Vector2f(0.4f, 0.7f),
            Position = Position,
            Scale = new Vector2f(MathF.Sign(Position.X - Player.Position.X), 1f) * 0.15f,
            Color = Color
        };

        Game.DrawSprite("reimu", states, Layers.Opponent);

        // var rect = new RectangleShape(new Vector2f(20f, 20f));
        // rect.Origin = rect.Size / 2f;
        // rect.Position = Position;
        // rect.FillColor = Color;
        // Game.Draw(rect, 0);
    }

    public override void DebugRender() {
        var circle = new CircleShape(5);
        circle.Origin = new Vector2f(circle.Radius, circle.Radius);

        circle.FillColor = new Color(0, 200, 255);
        circle.Position = basePosition;
        Game.Draw(circle, 0);

        circle.FillColor = new Color(255, 100, 0);
        circle.Position = predictedOffset;
        Game.Draw(circle, 0);

        circle.FillColor = new Color(0, 255, 0);
        circle.Position = interpolatedPosition;
        Game.Draw(circle, 0);


    }

    public override void PostRender() {
        predictedOffsetInterpolationTime = MathF.Min(predictedOffsetInterpolationTime + Game.Delta.AsSeconds() * 0.5f, 1f);
        smoothingOffsetInterpolationTime = MathF.Max(smoothingOffsetInterpolationTime - Game.Delta.AsSeconds(), 0f);

    }

    protected void AddAttack(PlayerAction action, Attack attack) => Attacks[action] = attack;

    protected void SpendPower(int amount) => powerSpent += amount;



    // packets
    private void VelocityChanged(Packet packet) {
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
    }



    private void AttackPressed(Packet packet) {

    }



    private void AttackReleased(Packet packet) {
        packet.Out(out PlayerAction action, true);
        if (Attacks.TryGetValue(action, out var attack)) {
            attack.OpponentReleased(this, packet);
        }
    }



    private void SpentPower(Packet packet) {
        packet.Out(out int amount, true);
        SpendPower(amount);
    }



    private void Grazed(Packet packet) {
        packet.Out(out int amount, true);
        powerGainedFromGrazing += amount;
    }



    private void Hit(Packet packet) {
        packet.Out(out Time theirTime, true).Out(out Vector2f theirPosition).Out(out float angle);

        Scene.AddEntity(new HitExplosion(theirPosition, 0.5f, 100f, Color));

        var latency = Game.Network.Time - theirTime;

        isHit = true;
        knockbackTime = Game.Time;
        knockbackStartPosition = theirPosition;
        knockbackEndPosition = theirPosition + new Vector2f(MathF.Cos(angle), MathF.Sin(angle)) * 100f;
        knockbackDuration = Time.InSeconds(1);

        HeartCount--;

        Game.Sounds.Play("hit");
    }



    private void Death(Packet packet) {
        isDead = true;
        packet.Out(out Time _, true).Out(out Vector2f theirPosition);

        Scene.AddEntity(new HitExplosion(theirPosition, 1f, 500f, Color));

        HeartCount--;

        Game.Sounds.Play("death");
    }








}