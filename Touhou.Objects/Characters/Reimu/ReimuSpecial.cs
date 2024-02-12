using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuSpecial : Attack<Reimu> {
    private readonly int numShots = 30;
    private readonly float velocity = 500;
    private readonly float deceleration = 500;
    private readonly Time spawnDuration = Time.InSeconds(0.15f);
    private readonly int grazeAmount = 1;

    public ReimuSpecial(Reimu c) : base(c) {
        Cost = 80;
    }

    public override void LocalPress(Time cooldownOverflow, bool focused) {


        var localGroup = new LocalTargetingAmuletGroup(c.IsP1, c.IsPlayer);
        c.Scene.AddEntity(localGroup);


        var arcAngle = MathF.Tau / numShots;

        var angle = c.AngleToOpponent + arcAngle / 2f;


        for (int i = 0; i < numShots; i++) {

            var projectile = new TargetingAmulet(c.Position, angle + arcAngle * i, c.IsP1, c.IsPlayer, false, velocity, deceleration) {
                SpawnDuration = spawnDuration,
                DestroyedOnScreenExit = false,
                CanCollide = false,
                Color = new Color4(0f, 1f, 0, 0.4f),
            };
            projectile.ForwardTime(cooldownOverflow, false);

            localGroup.Add(projectile);
            c.Scene.AddEntity(projectile);
        }

        c.ApplyAttackCooldowns(Time.InSeconds(1f), PlayerActions.Special);
        c.ApplyAttackCooldowns(Time.InSeconds(0.25f), PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.Super);

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.Special)
        .In(Game.Network.Time - cooldownOverflow)
        .In(c.Position)
        .In(angle);

        Game.Network.Send(packet);

        c.SpendPower(Cost);
    }



    public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {
    }



    public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
    }


    public override void RemoteRelease(Packet packet) {

        packet.Out(out Time time).Out(out Vector2 position).Out(out float angle);
        var latency = Game.Network.Time - time;


        var remoteGroup = new RemoteTargetingAmuletGroup(Time.InSeconds(1.5f), c.IsP1, c.IsPlayer);
        c.Scene.AddEntity(remoteGroup);


        var arcAngle = MathF.Tau / numShots;

        for (int i = 0; i < numShots; i++) {
            var projectile = new TargetingAmulet(position, angle + arcAngle * i, c.IsP1, c.IsPlayer, true, velocity, deceleration) {
                SpawnDuration = spawnDuration,
                DestroyedOnScreenExit = false,
                Color = new Color4(1f, 0, 0, 1f),
                GrazeAmount = grazeAmount
            };
            projectile.ForwardTime(latency, true);

            remoteGroup.Add(projectile);
            c.Scene.AddEntity(projectile);
        }
    }
}