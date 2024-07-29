using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public partial class Sakuya : Character {

    private class PrimaryLvl2 : Attack<Sakuya> {


        private readonly float velocity = 450f;
        private readonly float velocityChangePerWave = 80f;

        private readonly float deadzone = 24f;
        private readonly float deadzoneChangePerWave = 12f;

        private readonly int waveCount = 3;

        private readonly int spreadCount = 4;

        private readonly float angleBetweenSpreads = MathF.PI / 4f;
        private readonly int shotCountPerSpread = 3;
        private readonly float angleBetweenShots = 0.05f;


        private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);
        private float aimAngle;
        private bool isAiming;


        public PrimaryLvl2(Sakuya c) : base(c) {
            IsHoldable = true;
        }



        public override void LocalPress(Time cooldownOverflow, bool focused) {

            aimAngle = C.AngleToOpponent;

            C.DisableAttacks(
                PlayerActions.Primary,
                PlayerActions.Secondary,
                PlayerActions.Charge
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
                aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 5.5f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));
            } else {
                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(C.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.001f, Game.Delta.AsSeconds())));

            }
        }



        public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
            // var isTimestopped = C.GetEffect<Timestop>(out var timestop);

            bool isTimestopped = false;

            for (int wave = 0; wave < waveCount; wave++) {
                for (int spread = 0; spread < spreadCount; spread++) {

                    float spreadAngle = aimAngle + angleBetweenSpreads * spread - angleBetweenSpreads / 2f * (spreadCount - 1);

                    for (int shot = 0; shot < shotCountPerSpread; shot++) {

                        float angle = spreadAngle + angleBetweenShots * shot - angleBetweenShots / 2f * (shotCountPerSpread - 1);

                        var angleVector = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                        var offset = angleVector * (deadzone - deadzoneChangePerWave * wave);

                        var projectile = new LargeKnife(C.Position + offset, angle, velocity - velocityChangePerWave * wave, isTimestopped, C.IsP1, C.IsPlayer, false) {
                            SpawnDelay = Time.InSeconds(0.25f),
                            CanCollide = false,
                            Color = new Color4(0f, 1f, 0f, 0.4f)
                        };
                        projectile.IncreaseTime(cooldownOverflow, false);

                        // timestop?.AddProjectile(projectile);

                        C.Scene.AddEntity(projectile);
                    }
                }
            }

            C.EnableAttacks(
                PlayerActions.Primary,
                PlayerActions.Secondary,
                PlayerActions.Charge
            );

            C.ApplyAbilityLock(Time.InSeconds(0.25f) - cooldownOverflow, PlayerActions.Primary);
            C.ApplyAbilityLock(Time.InSeconds(0.25f) - cooldownOverflow, PlayerActions.Secondary);
            C.ApplyAbilityLock(Time.InSeconds(1.5f) - cooldownOverflow, PlayerActions.Special);

            isAiming = false;

            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.Special)
            .In(Game.Network.Time - cooldownOverflow)
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

            // var isTimestopped = C.GetEffect<Timestop>(out var timestop);

            bool isTimestopped = false;

            for (int wave = 0; wave < waveCount; wave++) {
                for (int spread = 0; spread < spreadCount; spread++) {

                    float spreadAngle = theirAngle + angleBetweenSpreads * spread - angleBetweenSpreads / 2f * (spreadCount - 1);

                    for (int shot = 0; shot < shotCountPerSpread; shot++) {

                        var angle = spreadAngle + this.angleBetweenShots * shot - this.angleBetweenShots / 2f * (shotCountPerSpread - 1);


                        var angleVector = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                        var offset = angleVector * (deadzone - deadzoneChangePerWave * wave);

                        var projectile = new LargeKnife(theirPosition + offset, angle, velocity - velocityChangePerWave * wave, isTimestopped, C.IsP1, C.IsPlayer, true) {
                            SpawnDelay = Time.InSeconds(0.25f),
                            Color = new Color4(1f, 0f, 0f, 1f),
                            GrazeAmount = 3,
                        };
                        projectile.IncreaseTime(latency, true);

                        // timestop?.AddProjectile(projectile);

                        C.Scene.AddEntity(projectile);
                    }
                }
            }

        }

        public override void Render() {

            if (!isAiming) return;

            var aimArrowSprite = new Sprite("aimarrow2") {
                Origin = new Vector2(-0.0625f, 0.5f),
                Position = C.Position,
                Rotation = aimAngle,
                Scale = new Vector2(0.3f),
                Color = new Color4(1f, 1f, 1f, 0.5f),
            };

            Game.Draw(aimArrowSprite, Layer.Player);


        }
    }
}