using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public partial class Marisa : Character {


    private class PrimaryLvl2 : Attack<Marisa> {
        private readonly int cost = 80;
        private readonly Time aimHoldTimeThreshhold;
        private readonly float laserWidth = 300f;
        private readonly Time startupTime = Time.InSeconds(1.1f);
        private readonly Time activeTime = Time.InSeconds(0.9f);
        private readonly int grazeAmount = 24;
        private bool isAiming;
        private float aimAngle;
        private Vector2 aimAngleVector;

        public PrimaryLvl2(Marisa c) : base(c) {
            IsHoldable = true;
        }



        public override void LocalPress(Time cooldownOverflow, bool focused) {

            aimAngle = C.AngleToOpponent;
            aimAngleVector = new Vector2(MathF.Cos(aimAngle), MathF.Sin(aimAngle));

            C.DisableAttacks(
                PlayerActions.Primary,
                PlayerActions.Secondary,
                PlayerActions.Special
            );
        }



        public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {

            if (holdTime < aimHoldTimeThreshhold) return;

            isAiming = true;

            float targetAngle = MathF.Atan2(C.Velocity.Y, C.Velocity.X);
            bool isMoving = (C.Velocity.X != 0f || C.Velocity.Y != 0f);
            float angleFromTarget = TMathF.NormalizeAngle(targetAngle - aimAngle);

            if (isMoving) {

                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(C.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.1f, Game.Delta.AsSeconds())));
                aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 0.6f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));

            } else {

                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(C.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.1f, Game.Delta.AsSeconds())));

            }

            aimAngleVector = new Vector2(MathF.Cos(aimAngle), MathF.Sin(aimAngle));
        }



        public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
            var laser = new Laser(C.Position, aimAngle, laserWidth, startupTime, activeTime, C.IsP1, C.IsPlayer, false) {
                Color = new Color4(0f, 1f, 0f, 0.4f),
                CanCollide = false
            };

            C.Scene.AddEntity(laser);
            laser.FowardTime(cooldownOverflow);

            C.ApplyMovespeedModifier(0f, startupTime + activeTime);


            C.ApplyAbilityLock(startupTime + activeTime + Time.InSeconds(0.2f) - cooldownOverflow,
                PlayerActions.Primary,
                PlayerActions.Secondary,
                PlayerActions.Special,
                PlayerActions.Charge
            );

            C.EnableAttacks(
                PlayerActions.Primary,
                PlayerActions.Secondary,
                PlayerActions.Special
            );

            isAiming = false;

            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.Charge)
            .In(Game.Network.Time)
            .In(C.Position)
            .In(aimAngle);

            Game.Network.Send(packet);

        }



        public override void RemoteRelease(Packet packet) {

            packet
            .Out(out Time theirTime)
            .Out(out Vector2 theirPosition)
            .Out(out float theirAngle);

            var latency = Game.Network.Time - theirTime;

            var laser = new Laser(theirPosition, theirAngle, laserWidth, startupTime, activeTime, C.IsP1, C.IsPlayer, true) {
                Color = new Color4(1f, 0f, 0f, 1f),
                CanCollide = false,
                GrazeAmount = grazeAmount
            };

            C.Scene.AddEntity(laser);
            laser.FowardTime(latency);


        }


        public override void Render() {

            if (!isAiming) return;

            var visualScale = laserWidth / 600f;

            var laserPreview = new Sprite("laser_indicator") {
                Origin = new Vector2(0f, 0.5f),
                Position = C.Position + aimAngleVector * laserWidth / 2f,
                Rotation = aimAngle,
                Scale = new Vector2(10000f, visualScale),
                Color = new Color4(1f, 1f, 1f, 0.1f),
                UVPaddingOffset = new Vector2(-0.5f, 0f),
                UseColorSwapping = true,
            };

            var laserPreviewStart = new Sprite(laserPreview) {
                SpriteName = "laser_indicator_start",
                Origin = new Vector2(1f, 0.5f),
                Scale = new Vector2(visualScale),
            };

            Game.Draw(laserPreviewStart, Layer.Player);
            Game.Draw(laserPreview, Layer.Player);
        }
    }
}
