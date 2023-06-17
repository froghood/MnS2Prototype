using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuPrimary : Attack {

    private bool attackHold;
    private float normalizedAimOffset;
    private float aimOffset;

    // aiming
    private readonly float aimRange = 80f; // degrees
    private readonly float aimStrength = 0.2f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

    // pattern
    private readonly Time spawnDelay = Time.InSeconds(0.15f);
    private readonly int grazeAmount = 2;
    private readonly int numShots = 5;

    private readonly float unfocusedSpacing = 0.3f; // radians
    private readonly float unfocusedVelocity = 115f;

    private readonly float focusedSpacing = 18f; // pixels
    private readonly float focusedVelocity = 300f;

    private readonly float velocityFalloff = 0.25f;
    private readonly float startingVelocityModifier = 4f;


    private readonly Time primaryCooldown = Time.InSeconds(0.5f);
    private readonly Time secondaryCooldown = Time.InSeconds(0.5f);
    private readonly Time spellACooldown = Time.InSeconds(0.5f);
    private readonly Time spellBCooldown = Time.InSeconds(0.5f);

    public ReimuPrimary() {
        Focusable = true;
        Holdable = true;
    }

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        player.DisableAttacks(PlayerAction.Secondary, PlayerAction.SpellA, PlayerAction.SpellB);
    }

    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        float aimRangeRadians = MathF.PI / 180f * aimRange;
        float gamma = 1 - MathF.Pow(aimStrength, Game.Delta.AsSeconds());
        float velocityAngle = MathF.Atan2(player.Velocity.Y, player.Velocity.X);
        bool moving = (player.Velocity.X != 0 || player.Velocity.Y != 0);

        if (holdTime > aimHoldTimeThreshhold) { // 75ms / 4.5 frames
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

        if (focused) {
            for (int index = 0; index < numShots; index++) {
                var offset = new Vector2f(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (focusedSpacing * index - focusedSpacing / 2f * (numShots - 1));
                var projectile = new LinearAmulet(player.Position + offset, angle, false, cooldownOverflow) {
                    SpawnDelay = spawnDelay,
                    CanCollide = false,
                    Color = new Color(0, 255, 0, 100),
                    StartingVelocity = focusedVelocity * startingVelocityModifier,
                    GoalVelocity = focusedVelocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.CollisionGroups.Add(0);
                player.Scene.AddEntity(projectile);
            }
        } else {
            for (int index = 0; index < numShots; index++) {
                var projectile = new LinearAmulet(player.Position, angle + unfocusedSpacing * index - unfocusedSpacing / 2f * (numShots - 1), false, cooldownOverflow) {
                    SpawnDelay = spawnDelay,
                    CanCollide = false,
                    Color = new Color(0, 255, 0, 100),
                    StartingVelocity = unfocusedVelocity * startingVelocityModifier,
                    GoalVelocity = unfocusedVelocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.CollisionGroups.Add(0);
                player.Scene.AddEntity(projectile);
            }
        }

        player.ApplyCooldowns(primaryCooldown - cooldownOverflow, PlayerAction.Primary);
        player.ApplyCooldowns(secondaryCooldown - cooldownOverflow, PlayerAction.Secondary);
        player.ApplyCooldowns(spellACooldown - cooldownOverflow, PlayerAction.SpellA);
        player.ApplyCooldowns(spellBCooldown - cooldownOverflow, PlayerAction.SpellB);

        player.EnableAttacks(PlayerAction.Secondary, PlayerAction.SpellA, PlayerAction.SpellB);

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerAction.Primary)
        .In(Game.Network.Time - cooldownOverflow)
        .In(player.Position)
        .In(angle)
        .In(focused);

        Game.Network.Send(packet);
    }

    public override void OpponentReleased(Opponent opponent, Packet packet) {

        packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle).Out(out bool focused);
        Time delta = Game.Network.Time - theirTime;

        if (focused) {
            for (int index = 0; index < numShots; index++) {
                var offset = new Vector2f(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (focusedSpacing * index - focusedSpacing / 2f * (numShots - 1));
                var projectile = new LinearAmulet(position + offset, angle, true) {
                    SpawnDelay = spawnDelay,
                    InterpolatedOffset = delta.AsSeconds(),
                    Color = new Color(255, 0, 0),
                    GrazeAmount = grazeAmount,
                    StartingVelocity = focusedVelocity * startingVelocityModifier,
                    GoalVelocity = focusedVelocity,
                    VelocityFalloff = velocityFalloff
                };
                projectile.CollisionGroups.Add(1);
                opponent.Scene.AddEntity(projectile);
            }
        } else {
            for (int index = 0; index < numShots; index++) {
                var projectile = new LinearAmulet(position, angle + unfocusedSpacing * index - unfocusedSpacing / 2f * (numShots - 1), true) {
                    SpawnDelay = spawnDelay,
                    InterpolatedOffset = delta.AsSeconds(),
                    Color = new Color(255, 0, 0),
                    GrazeAmount = grazeAmount,
                    StartingVelocity = unfocusedVelocity * startingVelocityModifier,
                    GoalVelocity = unfocusedVelocity,
                    VelocityFalloff = velocityFalloff
                };
                projectile.CollisionGroups.Add(1);
                opponent.Scene.AddEntity(projectile);
            }
        }
    }

    public override void PlayerRender(Player player) {
        if (!attackHold) return;

        var indicatorStates = new SpriteStates() {
            Origin = new Vector2f(0.5f, 0.5f),
            Position = player.Position,
            Scale = new Vector2f(1f, 1f) * 0.35f,
            Color = new Color(255, 255, 255, 40),
        };

        var shader = new TShader("aimIndicator");
        shader.SetUniform("angle", player.AngleToOpponent);
        shader.SetUniform("arc", TMathF.degToRad(aimRange));

        Game.DrawSprite("aimindicator", indicatorStates, shader, Layers.Player);

        byte darkness = (byte)MathF.Round(255f - 100f * MathF.Abs(normalizedAimOffset));

        var arrowStates = new SpriteStates() {
            Origin = new Vector2f(10f, 10f),
            OriginType = OriginType.Position,
            Position = player.Position,
            Rotation = TMathF.radToDeg(player.AngleToOpponent + aimOffset),
            Scale = new Vector2f(1f, 1f) * 0.35f,
            Color = new Color(255, darkness, darkness)
        };

        Game.DrawSprite("aimarrow", arrowStates, Layers.Player);
    }


}