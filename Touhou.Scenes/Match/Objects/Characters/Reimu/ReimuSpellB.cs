using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Scenes.Match.Objects.Characters;

public class ReimuSpellB : Attack {

    private bool attackHold;
    private float normalizedAimOffset;
    private float aimOffset;

    // aiming
    private readonly float aimRange = 140f; // degrees
    private readonly float aimStrength = 0.1f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

    // pattern
    private readonly float unfocusedVelocity = 100f;
    private readonly float unfocusedSize = 20f;

    private readonly float focusedVelocity = 50f;
    private readonly float focusedSize = 30f;

    private readonly Time globalCooldown = Time.InSeconds(0.15f);
    private readonly Time spellCooldown = Time.InSeconds(1f);

    public ReimuSpellB() {
        Holdable = true;
        Focusable = true;

    }



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        player.DisableAttacks("Primary", "Secondary", "SpellA");
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        float aimRangeRadians = MathF.PI / 180f * aimRange;
        float gamma = 1 - MathF.Pow(aimStrength, Game.Delta.AsSeconds());
        float velocityAngle = MathF.Atan2(player.Velocity.Y, player.Velocity.X);
        bool moving = (player.Velocity.X != 0 || player.Velocity.Y != 0);

        if (holdTime > aimHoldTimeThreshhold) {
            attackHold = true;
            var arcLengthToVelocity = TMathF.NormalizeAngle(velocityAngle - TMathF.NormalizeAngle(player.AngleToOpponent + normalizedAimOffset * aimRangeRadians));
            if (moving) {
                normalizedAimOffset -= normalizedAimOffset * gamma;
                normalizedAimOffset += MathF.Abs(arcLengthToVelocity / aimRangeRadians) < gamma ? arcLengthToVelocity / aimRangeRadians : gamma * MathF.Sign(arcLengthToVelocity);
                //_normalizedAimOffset += MathF.Min(gamma * MathF.Sign(arcLengthToVelocity), arcLengthToVelocity / aimRange);
            } else {
                normalizedAimOffset -= normalizedAimOffset * 0.1f;
            }
        } else {
            attackHold = false;
        }

        aimOffset = normalizedAimOffset * aimRangeRadians;
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        float angle = player.AngleToOpponent + aimOffset;

        var projectile = new YinYang(player.Position, angle, focused ? focusedSize : unfocusedSize, cooldownOverflow) {
            CanCollide = false,
            Color = new Color(0, 255, 0, 100),
            Velocity = focused ? focusedVelocity : unfocusedVelocity,
        };

        projectile.CollisionFilters.Add(0);
        player.SpawnProjectile(projectile);

        player.ApplyCooldowns(globalCooldown - cooldownOverflow, "Primary", "Secondary", "SpellA");
        player.ApplyCooldowns(spellCooldown - cooldownOverflow, "SpellB");

        player.EnableAttacks("Primary", "Secondary", "SpellA");

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;

        var packet = new Packet(PacketType.SpellB).In(Game.Network.Time - cooldownOverflow).In(player.Position).In(angle).In(focused);
        Game.Network.Send(packet);
    }



    public override void OpponentPress(Opponent opponent, Packet packet) {
        packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle).Out(out bool focused);
        Time delta = Game.Network.Time - theirTime;

        var projectile = new YinYang(position, angle, focused ? focusedSize : unfocusedSize) {
            InterpolatedOffset = delta.AsSeconds(),
            Color = new Color(255, 0, 0),
            Velocity = focused ? focusedVelocity : unfocusedVelocity,
        };

        projectile.CollisionFilters.Add(1);
        opponent.Scene.AddEntity(projectile);
    }
}