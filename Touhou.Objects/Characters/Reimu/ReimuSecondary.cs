using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuSecondary : Attack<Reimu> {

    private readonly float velocity = 200f;
    private readonly float turnRadius = 150f;
    private readonly float hitboxRadius = 10f;
    private readonly Time spawnDuration = Time.InSeconds(0.25f);
    private readonly Time preHomingDuration = Time.InSeconds(0.5f);
    private readonly Time homingDuration = Time.InSeconds(4f);
    private readonly float[] angles = { -0.4f, -0.133f, 0.133f, 0.4f };





    private readonly float aimRange = 120f; // degrees
    private readonly float aimStrength = 0.15f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

    private bool attackHold;
    private float normalizedAimOffset;
    private float aimOffset;


    // cooldowns
    private readonly Time primaryCooldown = Time.InSeconds(0.5f);
    private readonly Time secondaryCooldown = Time.InSeconds(2.5f);
    private readonly Time specialCooldown = Time.InSeconds(0.5f);
    private readonly Time superCooldown = Time.InSeconds(0.5f);



    public ReimuSecondary(Reimu c) : base(c) {
        IsHoldable = true;

        Icon = "reimu_secondary";
    }

    public override void LocalPress(Time cooldownOverflow, bool focused) {
        c.DisableAttacks(PlayerActions.Primary, PlayerActions.Special, PlayerActions.Super);
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
                normalizedAimOffset -= normalizedAimOffset * 0.1f;
            }
        } else {
            attackHold = false;
        }

        aimOffset = normalizedAimOffset * aimRangeRadians;
    }



    public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {

        foreach (var angle in angles) {
            var projectile = new LocalHomingAmulet(c.Position, c.AngleToOpponent + aimOffset + angle, turnRadius, velocity, hitboxRadius, c.IsP1, c.IsPlayer) {
                SpawnDuration = spawnDuration,
                PreHomingDuration = preHomingDuration,
                HomingDuration = homingDuration,

                Color = new Color4(0.4f, 1f, 0.667f, 0.4f),
                CanCollide = false,
            };

            c.Scene.AddEntity(projectile);
        }

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.Secondary)
        .In(Game.Network.Time)
        .In(c.Position)
        .In(c.AngleToOpponent + aimOffset);

        Game.Network.Send(packet);



        c.ApplyAttackCooldowns(primaryCooldown - cooldownOverflow, PlayerActions.Primary);
        c.ApplyAttackCooldowns(secondaryCooldown - cooldownOverflow, PlayerActions.Secondary);
        c.ApplyAttackCooldowns(specialCooldown - cooldownOverflow, PlayerActions.Special);
        c.ApplyAttackCooldowns(superCooldown - cooldownOverflow, PlayerActions.Super);

        c.EnableAttacks(PlayerActions.Primary, PlayerActions.Special, PlayerActions.Super);

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;
    }



    public override void RemoteRelease(Packet packet) {
        packet.Out(out Time theirTime).Out(out Vector2 theirPosition).Out(out float theirAngle);
        var delta = Game.Network.Time - theirTime;

        foreach (var angle in angles) {
            var projectile = new RemoteHomingAmulet(theirPosition, theirAngle + angle, turnRadius, velocity, hitboxRadius, c.IsP1, c.IsPlayer) {

                SpawnDuration = spawnDuration,
                PreHomingDuration = preHomingDuration,
                HomingDuration = homingDuration,

                Color = new Color4(1f, 0.4f, 0.667f, 1f),
                GrazeAmount = 3,
            };

            c.Scene.AddEntity(projectile);
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