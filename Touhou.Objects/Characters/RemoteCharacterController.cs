using System.Net;
using OpenTK.Mathematics;
using Touhou.Networking;

namespace Touhou.Objects.Characters;


public class RemoteCharacterController<T> : Entity where T : Character {

    private T c;
    private Dictionary<PacketType, ReceiveCallback> receiveCallbacks;
    private bool hasRemoteMatchStated;




    private Vector2 basePosition;
    private List<(Vector2 Offset, Timer Timer, Func<float, float> Easing)> interpolations = new();
    private Vector2 knockbackStartPosition;
    private Vector2 knockbackEndPosition;


    public RemoteCharacterController(T c) {

        this.c = c;
        basePosition = c.Position;

        receiveCallbacks = new Dictionary<PacketType, ReceiveCallback>() {
            {PacketType.MatchStarted, (_) => hasRemoteMatchStated = true},
            {PacketType.VelocityChanged, VelocityChanged},
            {PacketType.AttackReleased, AttackReleased},
            {PacketType.BombPressed, BombPressed},
            {PacketType.SpentPower, SpentPower},
            {PacketType.Grazed, Grazed},
            {PacketType.Hit, Hit},
            {PacketType.Knockbacked, Knockbacked},
            {PacketType.Death, Death}
        };


    }

    public override void Update() {
        if (c.State == CharacterState.Free) UpdateMovement();
        if (c.State == CharacterState.Knockbacked) UpdateKnockback();
    }



    private void UpdateMovement() {

        basePosition += c.Velocity * Game.Delta.AsSeconds();
        basePosition = Vector2.Clamp(basePosition, -c.Match.Bounds, c.Match.Bounds);

        var position = basePosition;

        foreach (var interpolation in interpolations) {
            position += interpolation.Offset * interpolation.Easing(interpolation.Timer.RemainingRatio);
        }

        position = Vector2.Clamp(position, -c.Match.Bounds, c.Match.Bounds);

        c.SetPosition(position);
    }



    private void UpdateKnockback() {

        float t = 1f - c.KnockbackedTimer.RemainingRatio;
        c.SetPosition(knockbackStartPosition + (knockbackEndPosition - knockbackStartPosition) * Easing.Out(t, 5f));

        c.SetPosition(Vector2.Clamp(c.Position, -c.Match.Bounds, c.Match.Bounds));
    }



    public void Receive(Packet packet) {

        if (!hasRemoteMatchStated && packet.Type != PacketType.MatchStarted) return;

        if (!receiveCallbacks.TryGetValue(packet.Type, out var callback)) return;

        callback.Invoke(packet);
    }

    private void VelocityChanged(Packet packet) {

        c.SetState(CharacterState.Free);

        packet
        .Out(out Time time)
        .Out(out Vector2 position)
        .Out(out Vector2 velocity);

        basePosition = position;


        var latency = Game.NetworkOld.Time - time;
        var predictedPosition = basePosition + velocity * latency.AsSeconds();

        interpolations.Clear();

        interpolations.Add((predictedPosition - basePosition, new Timer(Time.InSeconds(1f)), e => Easing.InOut(1f - e, 2f)));
        interpolations.Add((c.Position - basePosition, new Timer(Time.InSeconds(0.25f)), e => Easing.In(e, 2f)));

        c.SetVelocity(velocity);
    }

    private void AttackReleased(Packet packet) {
        packet.Out(out PlayerActions action, true);

        c.ReleaseRemoteAttack(action, packet);

    }

    private void BombPressed(Packet packet) {
        c.PressRemoteBomb(packet);

        Game.Sounds.Play("spell");
        Game.Sounds.Play("bomb");
    }


    private void SpentPower(Packet packet) {
        packet.Out(out int amount, true);

        c.SpendPower(amount, false);
    }

    private void Grazed(Packet packet) {
        packet.Out(out int grazeAmount, true);

        c.Graze(grazeAmount);
    }

    private void Hit(Packet packet) {

        c.SetState(CharacterState.Hit);

        packet.Out(out Time time).Out(out Vector2 position);

        var latency = Game.NetworkOld.Time - time;

        c.SetPosition(position);

        c.ApplyInvulnerability(Time.InSeconds(2.5f) - latency);
        c.Hit();

        Scene.AddEntity(new HitExplosion(c.Position, 0.5f, 100f, c.Color));

        Game.Sounds.Play("hit");

    }

    private void Knockbacked(Packet packet) {

        c.SetState(CharacterState.Knockbacked);

        packet.Out(out Time time).Out(out Vector2 position).Out(out float angle);

        var latency = Game.NetworkOld.Time - time;

        c.Damage();
        c.Knockback(Time.InSeconds(1f) - latency);

        angle += MathF.PI;
        float strength = 100f;

        knockbackStartPosition = c.Position;
        knockbackEndPosition = c.Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * strength;

        if (c.HeartCount == 1) Game.Sounds.Play("low_hearts");
    }

    private void Death(Packet packet) {

        c.SetState(CharacterState.Dead);

        packet.Out(out Time deathTime).Out(out Vector2 position);

        c.Damage();

        c.Die(deathTime);

        Scene.AddEntity(new HitExplosion(c.Position, 1f, 500f, c.Color));

        Game.Sounds.Play("death");
    }
}
