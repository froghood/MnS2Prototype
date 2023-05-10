using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Scenes.Match.Objects.Characters;

public class ReimuSpellA : Attack {

    private float startingAngle;

    private float angleOffset;
    private float angleOffsetVelocity;

    private Time timeThreshold;

    // pattern
    private readonly Time rateOfFire = Time.InSeconds(0.1f);
    private readonly float angleOffsetAcceleration = 1f;

    private readonly int numShots = 5;
    private readonly float velocity = 225f;

    private readonly float startingVelocityModifier = 2f;
    private readonly float velocityFalloff = 0.25f;

    private readonly Time spellCooldown = Time.InSeconds(0.5f);
    private readonly Time globalCooldown = Time.InSeconds(0.15f);

    public ReimuSpellA() {
        Holdable = true;
    }



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        startingAngle = player.AngleToOpponent;

        System.Console.WriteLine(startingAngle);

        angleOffsetVelocity = 0f;
        angleOffset = 0f;
        timeThreshold = Game.Time - cooldownOverflow;

        player.MovespeedModifier = 0.2f;

        player.DisableAttacks("Primary", "Secondary", "SpellB");
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {

        while (Game.Time >= timeThreshold) {

            Time timeOffset = Game.Time - timeThreshold;
            timeThreshold += rateOfFire;

            float angle = startingAngle + angleOffset / 360f * MathF.Tau + MathF.PI;

            for (int i = 0; i < numShots; i++) {
                var projectile = new LinearAmulet(player.Position, angle + MathF.Tau / numShots * i, cooldownOverflow + timeOffset) {
                    CanCollide = false,
                    Color = new Color(0, 255, 0, 100),
                    StartingVelocity = velocity * startingVelocityModifier,
                    GoalVelocity = velocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.CollisionFilters.Add(0);
                player.SpawnProjectile(projectile);
            }

            var packet = new Packet(PacketType.SpellA).In(Game.Network.Time - cooldownOverflow + timeOffset).In(player.Position).In(angle);
            Game.Network.Send(packet);

            angleOffsetVelocity += angleOffsetAcceleration;
            angleOffset += angleOffsetVelocity;
        }
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        player.MovespeedModifier = 1f;

        player.ApplyCooldowns(spellCooldown, "SpellA");
        player.ApplyCooldowns(globalCooldown, "Primary", "Secondary", "SpellB");

        player.EnableAttacks("Primary", "Secondary", "SpellB");
    }



    public override void OpponentPress(Opponent opponent, Packet packet) {
        packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle);
        var delta = Game.Network.Time - theirTime;

        for (int i = 0; i < numShots; i++) {
            var projectile = new LinearAmulet(position, angle + MathF.Tau / numShots * i) {
                InterpolatedOffset = delta.AsSeconds(),

                Color = new Color(255, 0, 0),

                StartingVelocity = velocity * startingVelocityModifier,
                GoalVelocity = velocity,
                VelocityFalloff = velocityFalloff,
            };
            projectile.CollisionFilters.Add(1);
            opponent.Scene.AddEntity(projectile);
        }
    }
}