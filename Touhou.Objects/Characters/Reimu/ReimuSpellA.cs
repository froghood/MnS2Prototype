using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuSpellA : Attack {
    private readonly int numShots = 30;
    private readonly float velocity = 600;
    private readonly float deceleration = 900;
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

            var projectile = new TargetingAmulet(player.Position, angle + arcAngle * i, false, velocity, deceleration, cooldownOverflow) {
                SpawnDelay = spawnDelay,
                DestroyedOnScreenExit = false,
                CanCollide = false,
                Color = new Color(0, 255, 0, 100),
            };
            localGroup.Add(projectile);
            player.Scene.AddEntity(projectile);
        }

        player.ApplyCooldowns(Time.InSeconds(1f), PlayerAction.SpellA);
        player.ApplyCooldowns(Time.InSeconds(0.25f), PlayerAction.Primary, PlayerAction.Secondary, PlayerAction.SpellB);

        player.SpendPower(Cost);

        var packet = new Packet(PacketType.SpellA).In(Game.Network.Time - cooldownOverflow).In(player.Position).In(angle);
        Game.Network.Send(packet);
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        throw new NotImplementedException();
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        throw new NotImplementedException();
    }


    public override void OpponentPress(Opponent opponent, Packet packet) {

        packet.Out(out Time time, true).Out(out Vector2f position).Out(out float angle);
        var delta = Game.Network.Time - time;


        var remoteGroup = new RemoteTargetingAmuletGroup(Time.InSeconds(1.5f), delta);
        opponent.Scene.AddEntity(remoteGroup);


        var arcAngle = MathF.Tau / numShots;

        for (int i = 0; i < numShots; i++) {
            var projectile = new TargetingAmulet(position, angle + arcAngle * i, true, velocity, deceleration) {
                SpawnDelay = spawnDelay,
                InterpolatedOffset = delta.AsSeconds(),
                DestroyedOnScreenExit = false,
                Color = new Color(255, 0, 0),
                GrazeAmount = grazeAmount
            };
            projectile.CollisionGroups.Add(1);
            remoteGroup.Add(projectile);
            opponent.Scene.AddEntity(projectile);
        }
    }
}