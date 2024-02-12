using System.Net;
using OpenTK.Mathematics;
using Touhou.Networking;

namespace Touhou.Objects.Characters;


public class RemoteCharacterController<T> : Entity, IReceivable where T : Character {

    private T c;
    private Dictionary<PacketType, Action<Packet>> receiveCallbacks;
    private bool hasRemoteMatchStated;

    public RemoteCharacterController(T c) {

        this.c = c;

        receiveCallbacks = new Dictionary<PacketType, Action<Packet>>() {
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







    public void Receive(Packet packet, IPEndPoint endPoint) {

        if (!hasRemoteMatchStated && packet.Type != PacketType.MatchStarted) return;

        if (!receiveCallbacks.TryGetValue(packet.Type, out var callback)) return;

        callback.Invoke(packet);
    }

    private void VelocityChanged(Packet packet) {
        packet
        .Out(out Time time)
        .Out(out Vector2 position)
        .Out(out Vector2 velocity);

        var latency = Game.Network.Time - time;
        var predictedPosition = position + velocity * latency.AsSeconds();

        c.ResetInterpolations();

        c.AddInterpolation(predictedPosition - position, Time.InSeconds(1f), e => Easing.InOut(1f - e, 2f));
        c.AddInterpolation(c.Position - position, Time.InSeconds(0.25f), e => Easing.In(e, 2f));

        c.SetPosition(position);
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

        packet.Out(out Time time).Out(out Vector2 position);

        var latency = Game.Network.Time - time;

        c.SetPosition(position);

        c.ApplyInvulnerability(Time.InSeconds(2.5f) - latency);
        c.Hit();

        Scene.AddEntity(new HitExplosion(c.Position, 0.5f, 100f, c.Color));

        Game.Sounds.Play("hit");

    }

    private void Knockbacked(Packet packet) {
        packet.Out(out Time time).Out(out Vector2 position).Out(out float angle);

        var latency = Game.Network.Time - time;

        c.Damage();

        if (c.HeartCount == 1) Game.Sounds.Play("low_hearts");

        c.Knockback(angle + MathF.PI, 100f, Time.InSeconds(1f) - latency);

    }

    private void Death(Packet packet) {
        packet.Out(out Time deathTime).Out(out Vector2 position);

        c.Damage();

        c.Die(deathTime);

        Scene.AddEntity(new HitExplosion(c.Position, 1f, 500f, c.Color));

        Game.Sounds.Play("death");
    }
}
