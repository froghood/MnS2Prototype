using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuSpellA : Attack {
    private readonly int numShots = 30;
    private readonly float velocity = 500;
    private readonly float deceleration = 600;
    private readonly Time spawnDelay = Time.InSeconds(0.15f);
    private readonly int grazeAmount = 1;

    public ReimuSpellA() {
        Cost = 80;
    }

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {


        var localGroup = new LocalTargetingAmuletGroup();
        player.Scene.AddEntity(localGroup);


        var arcAngle = MathF.Tau / numShots;

        var angle = player.AngleToOpponent + arcAngle / 2f;


        for (int i = 0; i < numShots; i++) {

            var projectile = new TargetingAmulet(player.Position, angle + arcAngle * i, true, false, velocity, deceleration, cooldownOverflow) {
                SpawnDelay = spawnDelay,
                DestroyedOnScreenExit = false,
                CanCollide = false,
                Color = new Color4(0f, 1f, 0, 0.4f),
            };
            localGroup.Add(projectile);
            player.Scene.AddEntity(projectile);
        }

        player.ApplyAttackCooldowns(Time.InSeconds(1f), PlayerActions.SpellA);
        player.ApplyAttackCooldowns(Time.InSeconds(0.25f), PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.SpellB);

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.SpellA)
        .In(Game.Network.Time - cooldownOverflow)
        .In(player.Position)
        .In(angle);

        Game.Network.Send(packet);

        player.SpendPower(Cost);
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        throw new NotImplementedException();
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        throw new NotImplementedException();
    }


    public override void OpponentReleased(Opponent opponent, Packet packet) {

        packet.Out(out Time time).Out(out Vector2 position).Out(out float angle);
        var delta = Game.Network.Time - time;


        var remoteGroup = new RemoteTargetingAmuletGroup(Time.InSeconds(1.5f), delta);
        opponent.Scene.AddEntity(remoteGroup);


        var arcAngle = MathF.Tau / numShots;

        for (int i = 0; i < numShots; i++) {
            var projectile = new TargetingAmulet(position, angle + arcAngle * i, false, true, velocity, deceleration) {
                SpawnDelay = spawnDelay,
                InterpolatedOffset = delta.AsSeconds(),
                DestroyedOnScreenExit = false,
                Color = new Color4(1f, 0, 0, 1f),
                GrazeAmount = grazeAmount
            };
            remoteGroup.Add(projectile);
            opponent.Scene.AddEntity(projectile);
        }
    }
}