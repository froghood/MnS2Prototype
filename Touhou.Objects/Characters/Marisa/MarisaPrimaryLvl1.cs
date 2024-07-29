using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public partial class Marisa : Character {
    private class PrimaryLvl1 : Attack<Marisa> {


        private static readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);
        private static readonly float laserWidth = 75f;
        private static readonly Time startupTime = Time.InSeconds(0.7f);
        private static readonly Time activeTime = Time.InSeconds(0.1f);
        private static readonly int grazeAmount = 8;



        private float aimAngle;
        private Vector2 aimAngleVector;
        private Vector2 unfocusedLaserPosition;
        private float unfocusedAngle;
        private bool isAiming;
        private bool isUnfocusedAiming;
        private bool isFocusedAiming;



        public PrimaryLvl1(Marisa c) : base(c) {
            IsHoldable = true;
            IsFocusable = true;
        }



        public override void LocalPress(Time cooldownOverflow, bool focused) {

            //Log.Info($"{this.GetType().Name} press: {cooldownOverflow.AsSeconds()}");

            aimAngle = C.AngleToOpponent;
            aimAngleVector = new Vector2(MathF.Cos(aimAngle), MathF.Sin(aimAngle));

            if (!focused) {
                CalculateUnfocusedLaserPositionAndAngle();
            }

            C.DisableAttacks(
                PlayerActions.Secondary,
                PlayerActions.Special,
                PlayerActions.Charge
            );
        }



        public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {

            //Log.Info($"{this.GetType().Name} hold: {cooldownOverflow.AsSeconds()}, {holdTime.AsSeconds()}");

            if (holdTime < aimHoldTimeThreshhold) return;

            isAiming = true;
            isUnfocusedAiming = !focused;
            isFocusedAiming = focused;

            float targetAngle = MathF.Atan2(C.Velocity.Y, C.Velocity.X);
            bool isMoving = (C.Velocity.X != 0f || C.Velocity.Y != 0f);
            float angleFromTarget = TMathF.NormalizeAngle(targetAngle - aimAngle);

            if (isMoving) {
                if (focused) {

                    aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(C.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.05f, Game.Delta.AsSeconds())));
                    aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 1.2f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));

                } else {

                    aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(C.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.1f, Game.Delta.AsSeconds())));
                    aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 2.2f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));
                }
            } else {
                if (focused) {

                    aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(C.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.0001f, Game.Delta.AsSeconds())));

                } else {

                    aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(C.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.05f, Game.Delta.AsSeconds())));
                }
            }

            aimAngleVector = new Vector2(MathF.Cos(aimAngle), MathF.Sin(aimAngle));

            if (!focused) {
                CalculateUnfocusedLaserPositionAndAngle();
            }
        }



        public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {

            //Log.Info($"{this.GetType().Name} release: {cooldownOverflow.AsSeconds()}, {heldTime.AsSeconds()}");


            Laser laser;

            if (focused) {
                laser = new Laser(C.Position, aimAngle, laserWidth, startupTime, activeTime, C.IsP1, C.IsPlayer, false) {
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                    CanCollide = false
                };

                C.Scene.AddEntity(laser);
                laser.FowardTime(cooldownOverflow);

                C.ApplyMovespeedModifier(0.3f, Time.InSeconds(0.3f));

            } else {

                var angle = MathF.Atan2(-MathF.Sign(unfocusedLaserPosition.Y), -MathF.Sign(unfocusedLaserPosition.X));

                laser = new Laser(unfocusedLaserPosition, unfocusedAngle, laserWidth, startupTime, activeTime, C.IsP1, C.IsPlayer, false) {
                    SpawnDeley = Time.InSeconds(1f),
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                    CanCollide = false
                };

                var sigil = new Sigil(C.Position, unfocusedLaserPosition, Time.InSeconds(1f), Time.InSeconds(1f), C.IsP1, C.IsPlayer, false) {
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                };

                C.Scene.AddEntity(laser);
                laser.FowardTime(cooldownOverflow);

                C.Scene.AddEntity(sigil);
                sigil.ForwardTime(cooldownOverflow, false);
            }

            var refundTime = Time.Min(heldTime, Time.InSeconds(0.3f));

            Log.Info(refundTime.AsSeconds());

            C.ApplyAbilityLock(Time.InSeconds(0.6f) - cooldownOverflow - refundTime, PlayerActions.Secondary);
            C.ApplyAbilityLock(Time.InSeconds(0.6f) - cooldownOverflow - refundTime, PlayerActions.Primary);

            C.ApplyAbilityLock(Time.InSeconds(0.2f) - cooldownOverflow,
                PlayerActions.Special,
                PlayerActions.Charge);

            C.EnableAttacks(
                PlayerActions.Secondary,
                PlayerActions.Special,
                PlayerActions.Charge);

            isAiming = false;

            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.Primary)
            .In(Game.Network.Time - cooldownOverflow)
            .In(focused)
            .In(C.Position);
            if (!focused) {
                packet.In(unfocusedLaserPosition);
            }
            packet.In(focused ? aimAngle : unfocusedAngle);

            Game.Network.Send(packet);


        }



        public override void RemoteRelease(Packet packet) {
            packet
            .Out(out Time theirTime)
            .Out(out bool focused);

            var latency = Game.Network.Time - theirTime;

            if (focused) {
                packet
                .Out(out Vector2 laserPosition)
                .Out(out float laserAngle);

                var laser = new Laser(laserPosition, laserAngle, laserWidth, startupTime, activeTime, C.IsP1, C.IsPlayer, true) {
                    Color = new Color4(1f, 0f, 0f, 1f),
                    CanCollide = false,
                    GrazeAmount = grazeAmount
                };

                C.Scene.AddEntity(laser);
                laser.FowardTime(latency);

            } else {
                packet
                .Out(out Vector2 theirPosition)
                .Out(out Vector2 laserPosition)
                .Out(out float laserAngle);

                var laser = new Laser(laserPosition, laserAngle, laserWidth, startupTime, activeTime, C.IsP1, C.IsPlayer, true) {
                    SpawnDeley = Time.InSeconds(1f),
                    Color = new Color4(1f, 0f, 0f, 1f),
                    CanCollide = false,
                    GrazeAmount = grazeAmount
                };

                var sigil = new Sigil(theirPosition, laserPosition, Time.InSeconds(1f), Time.InSeconds(1f), C.IsP1, C.IsPlayer, true) {
                    Color = new Color4(1f, 0f, 0f, 0.7f),
                };

                C.Scene.AddEntity(laser);
                laser.FowardTime(latency);

                C.Scene.AddEntity(sigil);
                sigil.ForwardTime(latency, true);
            }


        }



        public override void Render() {

            if (!isAiming) return;

            var visualScale = laserWidth / 600f;

            if (isFocusedAiming) {

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
                    UVPaddingOffset = Vector2.Zero
                };

                Game.Draw(laserPreviewStart, Layer.Player);
                Game.Draw(laserPreview, Layer.Player);

            } else {

                var unfocusedAngleVector = new Vector2(MathF.Cos(unfocusedAngle), MathF.Sin(unfocusedAngle));

                var laserPositionPreview = new Circle() {
                    Origin = new Vector2(0.5f),
                    Position = unfocusedLaserPosition,
                    Radius = 15f,
                    StrokeWidth = 4f,
                    StrokeColor = new Color4(1f, 1f, 1f, 0.5f),
                    FillColor = Color4.Transparent,
                };

                var laserPreview = new Sprite("laser_indicator") {
                    Origin = new Vector2(0f, 0.5f),
                    Position = unfocusedLaserPosition + unfocusedAngleVector * laserWidth / 2f,
                    Rotation = unfocusedAngle,
                    Scale = new Vector2(10000f, visualScale),
                    Color = new Color4(1f, 1f, 1f, 0.1f),
                    UVPaddingOffset = new Vector2(-0.5f, 0f),
                    UseColorSwapping = true,
                };

                var laserPreviewStart = new Sprite(laserPreview) {
                    SpriteName = "laser_indicator_start",
                    Origin = new Vector2(1f, 0.5f),
                    Scale = new Vector2(visualScale),
                    UVPaddingOffset = Vector2.Zero
                };

                var line = new Rectangle() {
                    Size = new Vector2((unfocusedLaserPosition - C.Position).Length - laserPositionPreview.Radius, 4f),
                    Origin = new Vector2(0f, 0.5f),
                    Position = C.Position,
                    Rotation = aimAngle,
                    StrokeWidth = 0f,
                    StrokeColor = Color4.Transparent,
                    FillColor = new Color4(1f, 1f, 1f, 0.25f),
                };

                Game.Draw(line, Layer.Player);
                Game.Draw(laserPreviewStart, Layer.Player);
                Game.Draw(laserPreview, Layer.Player);
                Game.Draw(laserPositionPreview, Layer.Player);

            }
        }



        private void CalculateUnfocusedLaserPositionAndAngle() {
            var cos = MathF.Cos(aimAngle);
            var sin = MathF.Sin(aimAngle);

            var distToVerticalWall = MathF.Max(
                (C.Match.Bounds.X - C.Position.X) / cos,
                (-C.Match.Bounds.X - C.Position.X) / cos);

            var distToHorizontalWall = MathF.Max(
                (C.Match.Bounds.Y - C.Position.Y) / sin,
                (-C.Match.Bounds.Y - C.Position.Y) / sin);

            var distToWall = MathF.Min(distToVerticalWall, distToHorizontalWall);

            unfocusedLaserPosition = C.Position + new Vector2(distToWall * cos, distToWall * sin);

            var halfPI = MathF.PI / 2f;

            if (distToHorizontalWall <= distToVerticalWall) {

                unfocusedAngle = halfPI * -MathF.Sign(unfocusedLaserPosition.Y);

            } else {

                unfocusedAngle = halfPI * MathF.Sign(unfocusedLaserPosition.X) + halfPI;

            }
        }
    }
}