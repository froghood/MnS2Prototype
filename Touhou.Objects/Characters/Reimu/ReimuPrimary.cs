using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
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
    private readonly Time specialACooldown = Time.InSeconds(0.5f);
    private readonly Time specialBCooldown = Time.InSeconds(0.5f);

    public ReimuPrimary() {
        Focusable = true;
        Holdable = true;
    }

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        player.DisableAttacks(PlayerActions.Secondary, PlayerActions.SpecialA, PlayerActions.SpecialB);
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
                normalizedAimOffset -= normalizedAimOffset * gamma;
            }
        } else {
            attackHold = false;
        }

        aimOffset = normalizedAimOffset * aimRangeRadians;
    }

    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        float angle = player.AngleToOpponent + aimOffset;

        //Game.Log("localprojectiles", $"@{(Game.Network.Time - cooldownOverflow).AsSeconds()}: {this.GetType().Name}");

        if (focused) {
            for (int index = 0; index < numShots; index++) {
                var offset = new Vector2(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (focusedSpacing * index - focusedSpacing / 2f * (numShots - 1));
                var projectile = new Amulet(player.Position + offset, angle, true, false) {
                    SpawnDelay = spawnDelay,
                    CanCollide = false,
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                    StartingVelocity = focusedVelocity * startingVelocityModifier,
                    GoalVelocity = focusedVelocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.ForwardTime(cooldownOverflow, false);

                player.Scene.AddEntity(projectile);
            }
        } else {
            for (int index = 0; index < numShots; index++) {
                var projectile = new Amulet(player.Position, angle + unfocusedSpacing * index - unfocusedSpacing / 2f * (numShots - 1), true, false) {
                    SpawnDelay = spawnDelay,
                    CanCollide = false,
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                    StartingVelocity = unfocusedVelocity * startingVelocityModifier,
                    GoalVelocity = unfocusedVelocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.ForwardTime(cooldownOverflow, false);

                player.Scene.AddEntity(projectile);
            }
        }

        player.ApplyAttackCooldowns(primaryCooldown - cooldownOverflow, PlayerActions.Primary);
        player.ApplyAttackCooldowns(secondaryCooldown - cooldownOverflow, PlayerActions.Secondary);
        player.ApplyAttackCooldowns(specialACooldown - cooldownOverflow, PlayerActions.SpecialA);
        player.ApplyAttackCooldowns(specialBCooldown - cooldownOverflow, PlayerActions.SpecialB);

        player.EnableAttacks(PlayerActions.Secondary, PlayerActions.SpecialA, PlayerActions.SpecialB);

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.Primary)
        .In(Game.Network.Time - cooldownOverflow)
        .In(player.Position)
        .In(angle)
        .In(focused);

        Game.Network.Send(packet);
    }

    public override void OpponentReleased(Opponent opponent, Packet packet) {

        packet.Out(out Time theirTime).Out(out Vector2 position).Out(out float angle).Out(out bool focused);
        Time delta = Game.Network.Time - theirTime;

        //Game.Log("localprojectiles", $"@{(theirTime).AsSeconds()}: {this.GetType().Name}");

        if (focused) {
            for (int index = 0; index < numShots; index++) {
                var offset = new Vector2(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (focusedSpacing * index - focusedSpacing / 2f * (numShots - 1));
                var projectile = new Amulet(position + offset, angle, false, true) {
                    SpawnDelay = spawnDelay,
                    Color = new Color4(1f, 0, 0, 1f),
                    GrazeAmount = grazeAmount,
                    StartingVelocity = focusedVelocity * startingVelocityModifier,
                    GoalVelocity = focusedVelocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.ForwardTime(delta, true);
                opponent.Scene.AddEntity(projectile);
            }
        } else {
            for (int index = 0; index < numShots; index++) {
                var projectile = new Amulet(position, angle + unfocusedSpacing * index - unfocusedSpacing / 2f * (numShots - 1), false, true) {
                    SpawnDelay = spawnDelay,
                    Color = new Color4(1f, 0, 0, 1f),
                    GrazeAmount = grazeAmount,
                    StartingVelocity = unfocusedVelocity * startingVelocityModifier,
                    GoalVelocity = unfocusedVelocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.ForwardTime(delta, true);
                opponent.Scene.AddEntity(projectile);
            }
        }
    }

    public override void PlayerRender(Player player) {

        if (!attackHold) return;

        float darkness = 1f - 0.4f * MathF.Abs(normalizedAimOffset);

        var aimArrowSprite = new Sprite("aimarrow2") {
            Origin = new Vector2(-0.0625f, 0.5f),
            Position = player.Position,
            Rotation = player.AngleToOpponent + aimOffset,
            Scale = new Vector2(0.3f),
            Color = new Color4(1f, darkness, darkness, 0.5f),
        };

        Game.Draw(aimArrowSprite, Layer.Player);
    }


}