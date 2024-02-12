using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuPrimary : Attack<Reimu> {

    private bool attackHold;
    private float normalizedAimOffset;
    private float aimOffset;

    // aiming
    private readonly float aimRange = 120f; // degrees
    private readonly float aimStrength = 0.12f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

    // pattern
    private readonly Time spawnDuration = Time.InSeconds(0.15f);
    private readonly int grazeAmount = 2;
    private readonly int numShots = 7;

    private readonly float unfocusedSpacing = 0.25f; // radians
    private readonly float unfocusedVelocity = 115f;

    private readonly float focusedSpacing = 20f; // pixels
    private readonly float focusedVelocity = 700f;

    private readonly float velocityFalloff = 1f;
    private readonly float startingVelocityModifier = 3f;


    private readonly Time primaryCooldown = Time.InSeconds(0.5f);
    private readonly Time secondaryCooldown = Time.InSeconds(0.5f);
    private readonly Time specialCooldown = Time.InSeconds(0.5f);
    private readonly Time superCooldown = Time.InSeconds(0.5f);

    public ReimuPrimary(Reimu c) : base(c) {

        IsHoldable = true;
        HasFocusVariant = true;

        Icon = "reimu_primary";
        FocusedIcon = "reimu_primary_focused";
    }

    public override void LocalPress(Time cooldownOverflow, bool focused) {
        c.DisableAttacks(PlayerActions.Secondary, PlayerActions.Special, PlayerActions.Super);
    }

    public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {
        float aimRangeRadians = MathF.PI / 180f * aimRange;
        float gamma = 1 - MathF.Pow(aimStrength, Game.Delta.AsSeconds());
        float velocityAngle = MathF.Atan2(c.Velocity.Y, c.Velocity.X);
        bool moving = (c.Velocity.X != 0 || c.Velocity.Y != 0);

        if (holdTime > aimHoldTimeThreshhold) { // 75ms / 4.5 frames
            attackHold = true;
            var arcLengthToVelocity = TMathF.NormalizeAngle(velocityAngle - TMathF.NormalizeAngle(c.AngleToOpponent + normalizedAimOffset * aimRangeRadians));
            if (moving) {
                normalizedAimOffset -= normalizedAimOffset * gamma;
                normalizedAimOffset += MathF.Abs(arcLengthToVelocity / aimRangeRadians) < gamma ? arcLengthToVelocity / aimRangeRadians : gamma * MathF.Sign(arcLengthToVelocity);
                //_normalizedAimOffset += MathF.Min(gamma * MathF.Sign(arcLengthToVelocity), arcLengthToVelocity / aimRange);
            } else {
                normalizedAimOffset -= normalizedAimOffset * gamma * 5f;
            }
        } else {
            attackHold = false;
        }

        aimOffset = normalizedAimOffset * aimRangeRadians;
    }

    public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
        float angle = c.AngleToOpponent + aimOffset;

        //Game.Log("localprojectiles", $"@{(Game.Network.Time - cooldownOverflow).AsSeconds()}: {this.GetType().Name}");

        if (focused) {
            for (int index = 0; index < numShots; index++) {
                var offset = new Vector2(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (focusedSpacing * index - focusedSpacing / 2f * (numShots - 1));
                var projectile = new Needle(c.Position + offset, angle, focusedVelocity, c.IsP1, c.IsPlayer, false) {

                    SpawnDelay = Time.InSeconds(0.02f * MathF.Abs(index - 3f)),
                    SpawnDuration = spawnDuration,
                    CanCollide = false,
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                };
                projectile.ForwardTime(cooldownOverflow, false);

                c.Scene.AddEntity(projectile);

                c.ApplyMovespeedModifier(0.6f, Time.InSeconds(0.4f) - cooldownOverflow);
            }
        } else {
            for (int index = 0; index < numShots; index++) {
                var projectile = new Amulet(c.Position, angle + unfocusedSpacing * index - unfocusedSpacing / 2f * (numShots - 1), c.IsP1, c.IsPlayer, false) {
                    SpawnDuration = spawnDuration,
                    CanCollide = false,
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                    StartingVelocity = unfocusedVelocity * startingVelocityModifier,
                    GoalVelocity = unfocusedVelocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.ForwardTime(cooldownOverflow, false);

                c.Scene.AddEntity(projectile);
            }
        }

        c.ApplyAttackCooldowns(primaryCooldown - cooldownOverflow, PlayerActions.Primary);
        c.ApplyAttackCooldowns(secondaryCooldown - cooldownOverflow, PlayerActions.Secondary);
        c.ApplyAttackCooldowns(specialCooldown - cooldownOverflow, PlayerActions.Special);
        c.ApplyAttackCooldowns(superCooldown - cooldownOverflow, PlayerActions.Super);

        c.EnableAttacks(PlayerActions.Secondary, PlayerActions.Special, PlayerActions.Super);

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.Primary)
        .In(Game.Network.Time - cooldownOverflow)
        .In(c.Position)
        .In(angle)
        .In(focused);

        Game.Network.Send(packet);
    }

    public override void RemoteRelease(Packet packet) {

        packet.Out(out Time theirTime).Out(out Vector2 position).Out(out float angle).Out(out bool focused);
        Time delta = Game.Network.Time - theirTime;

        //Game.Log("localprojectiles", $"@{(theirTime).AsSeconds()}: {this.GetType().Name}");

        if (focused) {
            for (int index = 0; index < numShots; index++) {
                var offset = new Vector2(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (focusedSpacing * index - focusedSpacing / 2f * (numShots - 1));
                var projectile = new Needle(position + offset, angle, focusedVelocity, c.IsP1, c.IsPlayer, true) {
                    SpawnDelay = Time.InSeconds(0.02f * MathF.Abs(index - 3f)),
                    SpawnDuration = spawnDuration,
                    Color = new Color4(1f, 0, 0, 1f),
                    GrazeAmount = grazeAmount,
                };
                projectile.ForwardTime(delta, true);
                c.Scene.AddEntity(projectile);
            }
        } else {
            for (int index = 0; index < numShots; index++) {
                var projectile = new Amulet(position, angle + unfocusedSpacing * index - unfocusedSpacing / 2f * (numShots - 1), c.IsP1, c.IsPlayer, true) {
                    SpawnDuration = spawnDuration,
                    Color = new Color4(1f, 0, 0, 1f),
                    GrazeAmount = grazeAmount,
                    StartingVelocity = unfocusedVelocity * startingVelocityModifier,
                    GoalVelocity = unfocusedVelocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.ForwardTime(delta, true);
                c.Scene.AddEntity(projectile);
            }
        }
    }

    public override void Render() {

        if (!attackHold) return;

        float darkness = 1f - 0.4f * MathF.Abs(normalizedAimOffset);

        var aimArrowSprite = new Sprite("aimarrow2") {
            Origin = new Vector2(-0.0625f, 0.5f),
            Position = c.Position,
            Rotation = c.AngleToOpponent + aimOffset,
            Scale = new Vector2(0.3f),
            Color = new Color4(1f, darkness, darkness, 0.5f),
        };

        Game.Draw(aimArrowSprite, Layer.Player);
    }


}