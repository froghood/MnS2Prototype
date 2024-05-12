using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class YukariSpecial : Attack<Character> {

    private float startingAngle;

    private float angleOffset;
    private float angleOffsetVelocity;

    private Time timeThreshold;

    // pattern
    private readonly int grazeAmount = 5;
    private readonly Time rateOfFire = Time.InSeconds(0.1f);
    private readonly float angleOffsetAcceleration = 1f;

    private readonly int numShots = 5;
    private readonly float velocity = 300f;

    private readonly float startingVelocityModifier = 2f;
    private readonly float velocityFalloff = 0.25f;

    private readonly Time specialCooldown = Time.InSeconds(1f);
    private readonly Time globalCooldown = Time.InSeconds(0.25f);

    public YukariSpecial(Character c) : base(c) {
        IsHoldable = true;
        Cost = 8;
    }



    public override void LocalPress(Time cooldownOverflow, bool focused) {
        startingAngle = c.AngleToOpponent;

        Log.Info(startingAngle);

        angleOffsetVelocity = 0f;
        angleOffset = 0f;
        timeThreshold = Game.Time - cooldownOverflow;

        c.ApplyMovespeedModifier(0.2f);

        c.DisableAttacks(PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.Super);
    }



    public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {

        while (Game.Time >= timeThreshold) {

            Time timeOffset = Game.Time - timeThreshold;
            timeThreshold += rateOfFire;

            float angle = startingAngle + angleOffset / 360f * MathF.Tau + MathF.PI;

            for (int i = 0; i < numShots; i++) {
                var projectile = new Amulet(c.Position, angle + MathF.Tau / numShots * i, c.IsP1, c.IsPlayer, false) {
                    CanCollide = false,
                    Color = new Color4(0, 1f, 0, 0.4f),
                    StartingVelocity = velocity * startingVelocityModifier,
                    GoalVelocity = velocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.ForwardTime(cooldownOverflow + timeOffset, false);

                c.Scene.AddEntity(projectile);

            }
            c.SpendPower(Cost);

            Game.NetworkOld.Send(
                PacketType.AttackReleased,
                PlayerActions.Special,
                Game.NetworkOld.Time - cooldownOverflow + timeOffset,
                c.Position,
                angle);

            angleOffsetVelocity += angleOffsetAcceleration;
            angleOffset += angleOffsetVelocity;
        }
    }



    public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
        c.ApplyMovespeedModifier(1f);

        c.ApplyAttackCooldowns(specialCooldown, PlayerActions.Special);
        c.ApplyAttackCooldowns(globalCooldown, PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.Super);

        c.EnableAttacks(PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.Super);
    }



    public override void RemoteRelease(Packet packet) {
        packet.Out(out Time theirTime).Out(out Vector2 position).Out(out float angle);
        var latency = Game.NetworkOld.Time - theirTime;

        for (int i = 0; i < numShots; i++) {
            var projectile = new Amulet(position, angle + MathF.Tau / numShots * i, c.IsP1, c.IsPlayer, true) {
                Color = new Color4(1f, 0f, 0f, 1f),
                GrazeAmount = grazeAmount,
                StartingVelocity = velocity * startingVelocityModifier,
                GoalVelocity = velocity,
                VelocityFalloff = velocityFalloff,
            };
            projectile.ForwardTime(latency, true);

            c.Scene.AddEntity(projectile);
        }
    }
}