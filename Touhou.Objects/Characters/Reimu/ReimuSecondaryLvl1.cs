using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public partial class Reimu : Character {
    private class SecondaryLvl1 : Attack<Reimu> {
        private const float velocity = 200f;
        private const float turnRadius = 150f;
        private const float hitboxRadius = 10f;
        private static readonly Time spawnDuration = Time.InSeconds(0.25f);
        private static readonly Time preHomingDuration = Time.InSeconds(0.5f);
        private static readonly Time homingDuration = Time.InSeconds(4f);
        private static readonly float[] angles = { -0.4f, -0.133f, 0.133f, 0.4f };
        private const float aimRange = 120f; // degrees
        private const float aimStrength = 0.15f;
        private static readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);


        private static readonly Time primaryLock = Time.InSeconds(0.5f);
        private static readonly Time secondaryLock = Time.InSeconds(2f);
        private static readonly Time specialLock = Time.InSeconds(0.5f);

        private bool attackHold;
        private float normalizedAimOffset;
        private float aimOffset;



        public SecondaryLvl1(Reimu c) : base(c) {
            IsHoldable = true;

        }

        public override void LocalPress(Time cooldownOverflow, bool focused) {
            C.DisableAttacks(PlayerActions.Primary, PlayerActions.Special);
        }



        public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {
            float aimRangeRadians = MathF.PI / 180f * aimRange;
            float gamma = 1 - MathF.Pow(aimStrength, Game.Delta.AsSeconds());
            float velocityAngle = MathF.Atan2(C.Velocity.Y, C.Velocity.X);
            bool moving = (C.Velocity.X != 0 || C.Velocity.Y != 0);

            if (holdTime > aimHoldTimeThreshhold) { // 75ms / 4.5 frames
                attackHold = true;
                var arcLengthToVelocity = TMathF.NormalizeAngle(velocityAngle - TMathF.NormalizeAngle(C.AngleToOpponent + normalizedAimOffset * aimRangeRadians));
                if (moving) {
                    normalizedAimOffset -= normalizedAimOffset * gamma;
                    normalizedAimOffset += MathF.Abs(arcLengthToVelocity / aimRangeRadians) < gamma ? arcLengthToVelocity / aimRangeRadians : gamma * MathF.Sign(arcLengthToVelocity);
                    //_normalizedAimOffset += MathF.Min(gamma * MathF.Sign(arcLengthToVelocity), arcLengthToVelocity / secondaryAimRange);
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
                var projectile = new LocalHomingAmulet(C.Position, C.AngleToOpponent + aimOffset + angle, turnRadius, velocity, hitboxRadius, C.IsP1, C.IsPlayer) {
                    SpawnDuration = spawnDuration,
                    PreHomingDuration = preHomingDuration,
                    HomingDuration = homingDuration,

                    Color = new Color4(0.4f, 1f, 0.667f, 0.4f),
                    CanCollide = false,
                };

                C.Scene.AddEntity(projectile);
            }

            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.Secondary)
            .In(Game.Network.Time)
            .In(C.Position)
            .In(C.AngleToOpponent + aimOffset);

            Game.Network.Send(packet);



            C.ApplyAbilityLock(primaryLock - cooldownOverflow, PlayerActions.Primary);
            C.ApplyAbilityLock(secondaryLock - cooldownOverflow, PlayerActions.Secondary);
            C.ApplyAbilityLock(specialLock - cooldownOverflow, PlayerActions.Special);

            C.EnableAttacks(PlayerActions.Primary, PlayerActions.Special, PlayerActions.Charge);

            attackHold = false;
            aimOffset = 0f;
            normalizedAimOffset = 0f;
        }



        public override void RemoteRelease(Packet packet) {
            packet.Out(out Time theirTime).Out(out Vector2 theirPosition).Out(out float theirAngle);
            var delta = Game.Network.Time - theirTime;

            foreach (var angle in angles) {
                var projectile = new RemoteHomingAmulet(theirPosition, theirAngle + angle, turnRadius, velocity, hitboxRadius, C.IsP1, C.IsPlayer) {

                    SpawnDuration = spawnDuration,
                    PreHomingDuration = preHomingDuration,
                    HomingDuration = homingDuration,

                    Color = new Color4(1f, 0.4f, 0.667f, 1f),
                    GrazeAmount = 3,
                };

                C.Scene.AddEntity(projectile);
            }
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
        }
    }
}