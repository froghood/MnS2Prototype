using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;


public partial class Reimu : Character {
    private class SpecialLvl1 : Attack<Reimu> {

        // aiming
        private bool attackHold;
        private float normalizedAimOffset;
        private float aimOffset;
        private float chargeTime = 0f;
        private readonly float aimRange = 40f; // degrees
        private readonly float aimStrength = 0.1f;
        private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

        // pattern
        private readonly int grazeAmount = 20;
        private readonly Time maxChargeTime = Time.InSeconds(1.5f);

        private readonly float minRadius = 30f;
        private readonly float maxRadius = 72f;
        private readonly float minVelocity = 120f;
        private readonly float maxVelocity = 60f;



        // cooldowns
        private readonly Time primaryCooldown = Time.InSeconds(0.5f);
        private readonly Time secondaryCooldown = Time.InSeconds(0.5f);
        private readonly Time specialCooldown = Time.InSeconds(0.5f);
        private readonly Time superCooldown = Time.InSeconds(1f);



        public SpecialLvl1(Reimu c) : base(c) {
            IsHoldable = true;
        }



        public override void LocalPress(Time cooldownOverflow, bool focused) {
            C.DisableAttacks(PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.Special);

            C.ApplyMovespeedModifier(0.1f);


        }



        public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {
            float aimRangeRadians = MathF.PI / 180f * aimRange;
            float gamma = 1 - MathF.Pow(aimStrength, Game.Delta.AsSeconds());
            float velocityAngle = MathF.Atan2(C.Velocity.Y, C.Velocity.X);
            bool moving = (C.Velocity.X != 0 || C.Velocity.Y != 0);

            if (holdTime > aimHoldTimeThreshhold) {
                attackHold = true;
                var arcLengthToVelocity = TMathF.NormalizeAngle(velocityAngle - TMathF.NormalizeAngle(C.AngleToOpponent + normalizedAimOffset * aimRangeRadians));
                if (moving) {
                    normalizedAimOffset -= normalizedAimOffset * gamma;
                    normalizedAimOffset += MathF.Abs(arcLengthToVelocity / aimRangeRadians) < gamma ? arcLengthToVelocity / aimRangeRadians : gamma * MathF.Sign(arcLengthToVelocity);
                    //_normalizedAimOffset += MathF.Min(gamma * MathF.Sign(arcLengthToVelocity), arcLengthToVelocity / aimRange);
                } else {
                    normalizedAimOffset -= normalizedAimOffset * 0.1f;
                }

                chargeTime = MathF.Min(chargeTime + Game.Delta.AsSeconds() / maxChargeTime.AsSeconds(), 1f);


            } else {
                attackHold = false;
            }

            aimOffset = normalizedAimOffset * aimRangeRadians;
        }



        public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
            float angle = C.AngleToOpponent + aimOffset;

            // var projectile = new YinYang(C.Position, angle, false, focused ? focusedSize : unfocusedSize, cooldownOverflow) {
            //     CanCollide = false,
            //     Color4 = new Color4(0, 255, 0, 100),
            //     Velocity = focused ? focusedVelocity : unfocusedVelocity,
            // };

            var radius = minRadius + (maxRadius - minRadius) * chargeTime;
            var velocity = minVelocity + (maxVelocity - minVelocity) * chargeTime;

            var projectile = new YinYang(C.Position, angle, C.IsP1, C.IsPlayer, false, radius) {
                CanCollide = false,
                Color = new Color4(0f, 1f, 0f, 0.4f),
                Velocity = velocity,
                SpawnDuration = Time.InSeconds(0.5f),
            };
            projectile.ForwardTime(cooldownOverflow, false);

            C.Scene.AddEntity(projectile);

            C.ApplyAbilityLock(primaryCooldown - cooldownOverflow, PlayerActions.Primary);
            C.ApplyAbilityLock(secondaryCooldown - cooldownOverflow, PlayerActions.Secondary);
            C.ApplyAbilityLock(specialCooldown - cooldownOverflow, PlayerActions.Special);

            C.EnableAttacks(PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.Special);

            C.ApplyMovespeedModifier(1f);

            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.Charge)
            .In(Game.Network.Time - cooldownOverflow)
            .In(C.Position)
            .In(angle)
            .In(radius).In(velocity);

            Game.Network.Send(packet);

            attackHold = false;
            aimOffset = 0f;
            normalizedAimOffset = 0f;

            chargeTime = 0f;
        }



        public override void RemoteRelease(Packet packet) {
            packet.Out(out Time theirTime).Out(out Vector2 position).Out(out float angle).Out(out float size).Out(out float velocity);
            Time delta = Game.Network.Time - theirTime;

            var projectile = new YinYang(position, angle, C.IsP1, C.IsPlayer, true, size) {
                Color = new Color4(1f, 0f, 0f, 1f),
                GrazeAmount = grazeAmount,
                Velocity = velocity,
                SpawnDuration = Time.InSeconds(0.5f),
            };
            projectile.ForwardTime(delta, true);

            C.Scene.AddEntity(projectile);
        }

        public override void Render() {
            if (!attackHold) return;

            float darkness = 1f - 0.4f * MathF.Abs(normalizedAimOffset);

            var aimArrowSprite = new Sprite("aimarrow2") {
                Origin = new Vector2(-0.0625f, 0.5f),
                Position = C.Position,
                Rotation = C.AngleToOpponent + aimOffset,
                Scale = new Vector2(0.3f),
                Color = new Color4(1f, darkness, darkness, 0.5f),
            };

            Game.Draw(aimArrowSprite, Layer.Player);

            var sizeIndicator = new Circle() {
                Origin = new Vector2(0.5f),
                Radius = minRadius + (maxRadius - minRadius) * chargeTime,
                Position = C.Position,
                StrokeWidth = 1f,
                StrokeColor = new Color4(1f, 1f, 1f, 0.4f),
                FillColor = Color4.Transparent,

            };

            Game.Draw(sizeIndicator, Layer.Player);
        }
    }
}
