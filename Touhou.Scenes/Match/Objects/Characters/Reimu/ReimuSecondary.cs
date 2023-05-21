using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Scenes.Match.Objects.Characters;

public class ReimuSecondary : Attack {

    // pattern
    private readonly int grazeAmount = 1;
    private readonly int numShots = 2;
    private readonly float spacing = 30f; // pixels
    private readonly float velocity = 350f;
    private readonly float startingVelocityModifier = 4f;
    private readonly float velocityFalloff = 0.25f;

    private readonly Time cooldown = Time.InSeconds(0.15f);

    public ReimuSecondary() {
        Holdable = true;
    }



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        player.DisableAttacks(PlayerAction.Primary, PlayerAction.SpellA, PlayerAction.SpellB);
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        float angle = player.AngleToOpponent;

        for (int index = 0; index < numShots; index++) {
            var offset = new Vector2f(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (spacing * index - spacing / 2f * (numShots - 1));
            var projectile = new LinearAmulet(player.Position + offset, angle, cooldownOverflow) {
                CanCollide = false,
                Color = new Color(0, 255, 0, 100),
                StartingVelocity = velocity * startingVelocityModifier,
                GoalVelocity = velocity,
                VelocityFalloff = velocityFalloff,
            };
            projectile.CollisionGroups.Add(0);
            player.SpawnProjectile(projectile);
        }

        player.ApplyCooldowns(cooldown - cooldownOverflow, PlayerAction.Secondary);

        var packet = new Packet(PacketType.Secondary).In(Game.Network.Time - cooldownOverflow).In(player.Position).In(angle);
        Game.Network.Send(packet);
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        player.EnableAttacks(PlayerAction.Primary, PlayerAction.SpellA, PlayerAction.SpellB);
    }



    public override void OpponentPress(Opponent opponent, Packet packet) {

        packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle);
        var delta = Game.Network.Time - theirTime;

        for (int index = 0; index < numShots; index++) {
            var offset = new Vector2f(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (spacing * index - spacing / 2f * (numShots - 1));
            var projectile = new LinearAmulet(position + offset, angle) {
                InterpolatedOffset = delta.AsSeconds(),

                Color = new Color(255, 0, 0),

                GrazeAmount = grazeAmount,

                StartingVelocity = velocity * startingVelocityModifier,
                GoalVelocity = velocity,
                VelocityFalloff = velocityFalloff
            };
            projectile.CollisionGroups.Add(1);

            opponent.SpawnProjectile(projectile);
        }
    }
}