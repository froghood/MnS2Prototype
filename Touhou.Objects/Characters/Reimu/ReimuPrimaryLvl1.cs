using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public partial class Reimu : Character {

    private class PrimaryLvl1 : Attack<Reimu> {


        private const float aimRange = 120f; // degrees
        private const float aimStrength = 0.12f;
        public readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

        // pattern
        private readonly Time spawnDuration = Time.InSeconds(0.15f);
        private const int grazeAmount = 2;
        private const int numShots = 7;

        private const float unfocusedSpacing = 0.25f; // radians
        private const float unfocusedVelocity = 115f;

        private const float focusedSpacing = 20f; // pixels
        private const float focusedVelocity = 700f;

        private const float velocityFalloff = 1f;
        private const float startingVelocityModifier = 3f;

        private readonly Time primaryLock = Time.InSeconds(1f);
        private readonly Time secondaryLock = Time.InSeconds(0.5f);
        private readonly Time specialLock = Time.InSeconds(0.5f);

        private bool attackHold;
        private float normalizedAimOffset;
        private float aimOffset;


        public PrimaryLvl1(Reimu c) : base(c) {
            IsFocusable = true;
            IsHoldable = true;
        }

        public override void LocalPress(Time cooldownOverflow, bool focused) {
            C.DisableAttacks(PlayerActions.Secondary, PlayerActions.Special, PlayerActions.Charge);
        }

        public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {
            float primaryAimRangeRadians = MathF.PI / 180f * aimRange;
            float gamma = 1f - MathF.Pow(aimStrength, Game.Delta.AsSeconds());
            float velocityAngle = MathF.Atan2(C.Velocity.Y, C.Velocity.X);
            bool moving = (C.Velocity.X != 0 || C.Velocity.Y != 0);

            if (holdTime > aimHoldTimeThreshhold) { // 75ms / 4.5 frames
                attackHold = true;
                var arcLengthToVelocity = TMathF.NormalizeAngle(velocityAngle - TMathF.NormalizeAngle(C.AngleToOpponent + normalizedAimOffset * primaryAimRangeRadians));
                if (moving) {
                    normalizedAimOffset -= normalizedAimOffset * gamma;
                    normalizedAimOffset += MathF.Abs(arcLengthToVelocity / primaryAimRangeRadians) < gamma ? arcLengthToVelocity / primaryAimRangeRadians : gamma * MathF.Sign(arcLengthToVelocity);
                    //_normalizedAimOffset += MathF.Min(gamma * MathF.Sign(arcLengthToVelocity), arcLengthToVelocity / primaryAimRange);
                } else {
                    normalizedAimOffset -= normalizedAimOffset * gamma * 5f;
                }
            } else {
                attackHold = false;
            }

            aimOffset = normalizedAimOffset * primaryAimRangeRadians;
        }

        public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
            float angle = C.AngleToOpponent + aimOffset;

            //Game.Log("localprojectiles", $"@{(Game.Network.Time - cooldownOverflow).AsSeconds()}: {this.GetType().Name}");

            if (focused) {
                for (int index = 0; index < numShots; index++) {
                    var offset = new Vector2(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (focusedSpacing * index - focusedSpacing / 2f * (numShots - 1));
                    var projectile = new Needle(C.Position + offset, angle, focusedVelocity, C.IsP1, C.IsPlayer, false) {

                        SpawnDelay = Time.InSeconds(0.02f * MathF.Abs(index - 3f)),
                        SpawnDuration = spawnDuration,
                        CanCollide = false,
                        Color = new Color4(0f, 1f, 0f, 0.4f),
                    };
                    projectile.ForwardTime(cooldownOverflow, false);

                    C.Scene.AddEntity(projectile);

                    C.ApplyMovespeedModifier(0.6f, Time.InSeconds(0.4f) - cooldownOverflow);
                }
            } else {
                for (int index = 0; index < numShots; index++) {
                    var projectile = new Amulet(C.Position, angle + unfocusedSpacing * index - unfocusedSpacing / 2f * (numShots - 1), C.IsP1, C.IsPlayer, false) {
                        SpawnDuration = spawnDuration,
                        CanCollide = false,
                        Color = new Color4(0f, 1f, 0f, 0.4f),
                        StartingVelocity = unfocusedVelocity * startingVelocityModifier,
                        GoalVelocity = unfocusedVelocity,
                        VelocityFalloff = velocityFalloff,
                    };
                    projectile.ForwardTime(cooldownOverflow, false);

                    C.Scene.AddEntity(projectile);
                }
            }

            C.ApplyAbilityLock(primaryLock - cooldownOverflow, PlayerActions.Primary);
            C.ApplyAbilityLock(secondaryLock - cooldownOverflow, PlayerActions.Secondary);
            C.ApplyAbilityLock(specialLock - cooldownOverflow, PlayerActions.Special);

            C.EnableAttacks(PlayerActions.Secondary, PlayerActions.Special, PlayerActions.Charge);

            attackHold = false;
            aimOffset = 0f;
            normalizedAimOffset = 0f;

            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.Primary)
            .In(Game.Network.Time - cooldownOverflow)
            .In(C.Position)
            .In(angle)
            .In(focused);

            Game.Network.Send(packet);
        }

        private void RemotePrimaryLvl1Release(Packet packet) {

            packet.Out(out Time theirTime).Out(out Vector2 position).Out(out float angle).Out(out bool focused);
            Time delta = Game.Network.Time - theirTime;

            //Game.Log("localprojectiles", $"@{(theirTime).AsSeconds()}: {this.GetType().Name}");

            if (focused) {
                for (int index = 0; index < numShots; index++) {
                    var offset = new Vector2(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (focusedSpacing * index - focusedSpacing / 2f * (numShots - 1));
                    var projectile = new Needle(position + offset, angle, focusedVelocity, C.IsP1, C.IsPlayer, true) {
                        SpawnDelay = Time.InSeconds(0.02f * MathF.Abs(index - 3f)),
                        SpawnDuration = spawnDuration,
                        Color = new Color4(1f, 0, 0, 1f),
                        GrazeAmount = grazeAmount,
                    };
                    projectile.ForwardTime(delta, true);
                    C.Scene.AddEntity(projectile);
                }
            } else {
                for (int index = 0; index < numShots; index++) {
                    var projectile = new Amulet(position, angle + unfocusedSpacing * index - unfocusedSpacing / 2f * (numShots - 1), C.IsP1, C.IsPlayer, true) {
                        SpawnDuration = spawnDuration,
                        Color = new Color4(1f, 0, 0, 1f),
                        GrazeAmount = grazeAmount,
                        StartingVelocity = unfocusedVelocity * startingVelocityModifier,
                        GoalVelocity = unfocusedVelocity,
                        VelocityFalloff = velocityFalloff,
                    };
                    projectile.ForwardTime(delta, true);
                    C.Scene.AddEntity(projectile);
                }
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