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
    }



    public override void LocalPress(Time cooldownOverflow, bool focused) {
        startingAngle = C.AngleToOpponent;

        Log.Info(startingAngle);

        angleOffsetVelocity = 0f;
        angleOffset = 0f;
        timeThreshold = Game.Time - cooldownOverflow;

        C.ApplyMovespeedModifier(0.2f);

        C.DisableAttacks(PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.Charge);
    }



    public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {

        while (Game.Time >= timeThreshold) {

            Time timeOffset = Game.Time - timeThreshold;
            timeThreshold += rateOfFire;

            float angle = startingAngle + angleOffset / 360f * MathF.Tau + MathF.PI;

            for (int i = 0; i < numShots; i++) {
                var projectile = new Amulet(C.Position, angle + MathF.Tau / numShots * i, C.IsP1, C.IsPlayer, false) {
                    CanCollide = false,
                    Color = new Color4(0, 1f, 0, 0.4f),
                    StartingVelocity = velocity * startingVelocityModifier,
                    GoalVelocity = velocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.ForwardTime(cooldownOverflow + timeOffset, false);

                C.Scene.AddEntity(projectile);

            }

            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.Special)
            .In(Game.Network.Time - cooldownOverflow + timeOffset)
            .In(C.Position)
            .In(angle);

            Game.Network.Send(packet);

            angleOffsetVelocity += angleOffsetAcceleration;
            angleOffset += angleOffsetVelocity;
        }
    }



    public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
        C.ApplyMovespeedModifier(1f);

        C.ApplyAbilityLock(specialCooldown, PlayerActions.Special);
        C.ApplyAbilityLock(globalCooldown, PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.Charge);

        C.EnableAttacks(PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.Charge);
    }



    public override void RemoteRelease(Packet packet) {
        packet.Out(out Time theirTime).Out(out Vector2 position).Out(out float angle);
        var latency = Game.Network.Time - theirTime;

        for (int i = 0; i < numShots; i++) {
            var projectile = new Amulet(position, angle + MathF.Tau / numShots * i, C.IsP1, C.IsPlayer, true) {
                Color = new Color4(1f, 0f, 0f, 1f),
                GrazeAmount = grazeAmount,
                StartingVelocity = velocity * startingVelocityModifier,
                GoalVelocity = velocity,
                VelocityFalloff = velocityFalloff,
            };
            projectile.ForwardTime(latency, true);

            C.Scene.AddEntity(projectile);
        }
    }
}