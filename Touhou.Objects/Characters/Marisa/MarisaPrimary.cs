using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class MarisaPrimary : Attack {


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



    public MarisaPrimary() {
        Holdable = true;
    }



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        aimAngle = player.AngleToOpponent;
        aimAngleVector = new Vector2(MathF.Cos(aimAngle), MathF.Sin(aimAngle));

        if (!focused) {
            CalculateUnfocusedLaserPositionAndAngle(player);
        }

        player.DisableAttacks(
            PlayerActions.Secondary,
            PlayerActions.SpecialA,
            PlayerActions.SpecialB
        );
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        if (holdTime < aimHoldTimeThreshhold) return;

        isAiming = true;
        isUnfocusedAiming = !focused;
        isFocusedAiming = focused;

        float targetAngle = MathF.Atan2(player.Velocity.Y, player.Velocity.X);
        bool isMoving = (player.Velocity.X != 0f || player.Velocity.Y != 0f);
        float angleFromTarget = TMathF.NormalizeAngle(targetAngle - aimAngle);

        if (isMoving) {
            if (focused) {

                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.05f, Game.Delta.AsSeconds())));
                aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 1.2f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));

            } else {

                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.1f, Game.Delta.AsSeconds())));
                aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 2.2f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));
            }
        } else {
            if (focused) {

                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.0001f, Game.Delta.AsSeconds())));

            } else {

                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.05f, Game.Delta.AsSeconds())));
            }
        }

        aimAngleVector = new Vector2(MathF.Cos(aimAngle), MathF.Sin(aimAngle));

        if (!focused) {
            CalculateUnfocusedLaserPositionAndAngle(player);
        }
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        Laser laser;

        if (focused) {
            laser = new Laser(player.Position, aimAngle, laserWidth, startupTime, activeTime, true, false) {
                Color = new Color4(0f, 1f, 0f, 0.4f),
                CanCollide = false
            };

            player.Scene.AddEntity(laser);
            laser.FowardTime(cooldownOverflow);

            player.ApplyMovespeedModifier(0.3f, Time.InSeconds(0.3f));

        } else {

            var angle = MathF.Atan2(-MathF.Sign(unfocusedLaserPosition.Y), -MathF.Sign(unfocusedLaserPosition.X));

            laser = new Laser(unfocusedLaserPosition, unfocusedAngle, laserWidth, startupTime, activeTime, true, false) {
                SpawnDeley = Time.InSeconds(1f),
                Color = new Color4(0f, 1f, 0f, 0.4f),
                CanCollide = false
            };

            var sigil = new Sigil(player.Position, unfocusedLaserPosition, Time.InSeconds(1f), Time.InSeconds(1f), true, false) {
                Color = new Color4(0f, 1f, 0f, 0.4f),
            };

            player.Scene.AddEntity(laser);
            laser.FowardTime(cooldownOverflow);

            player.Scene.AddEntity(sigil);
            sigil.ForwardTime(cooldownOverflow, false);
        }

        var refundTime = Time.Min(heldTime, Time.InSeconds(0.3f));

        player.ApplyAttackCooldowns(Time.InSeconds(0.6f) - refundTime - cooldownOverflow, PlayerActions.Primary);
        player.ApplyAttackCooldowns(Time.InSeconds(0.6f) - refundTime - cooldownOverflow, PlayerActions.Secondary);

        player.ApplyAttackCooldowns(Time.InSeconds(0.2f) - cooldownOverflow,
            PlayerActions.SpecialA,
            PlayerActions.SpecialB);

        player.EnableAttacks(
            PlayerActions.Secondary,
            PlayerActions.SpecialA,
            PlayerActions.SpecialB);

        isAiming = false;

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.Primary)
        .In(Game.Network.Time - cooldownOverflow)
        .In(focused)
        .In(player.Position);
        if (!focused) {
            packet.In(unfocusedLaserPosition);
        }
        packet.In(focused ? aimAngle : unfocusedAngle);

        Game.Network.Send(packet);


    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {
        packet
        .Out(out Time theirTime)
        .Out(out bool focused);

        var latency = Game.Network.Time - theirTime;

        if (focused) {
            packet
            .Out(out Vector2 laserPosition)
            .Out(out float laserAngle);

            var laser = new Laser(laserPosition, laserAngle, laserWidth, startupTime, activeTime, false, true) {
                Color = new Color4(1f, 0f, 0f, 1f),
                CanCollide = false,
                GrazeAmount = grazeAmount
            };

            opponent.Scene.AddEntity(laser);
            laser.FowardTime(latency);

        } else {
            packet
            .Out(out Vector2 theirPosition)
            .Out(out Vector2 laserPosition)
            .Out(out float laserAngle);

            var laser = new Laser(laserPosition, laserAngle, laserWidth, startupTime, activeTime, false, true) {
                SpawnDeley = Time.InSeconds(1f),
                Color = new Color4(1f, 0f, 0f, 1f),
                CanCollide = false,
                GrazeAmount = grazeAmount
            };

            var sigil = new Sigil(theirPosition, laserPosition, Time.InSeconds(1f), Time.InSeconds(1f), false, true) {
                Color = new Color4(1f, 0f, 0f, 0.7f),
            };

            opponent.Scene.AddEntity(laser);
            laser.FowardTime(latency);

            opponent.Scene.AddEntity(sigil);
            sigil.ForwardTime(latency, true);
        }


    }



    public override void PlayerRender(Player player) {

        if (!isAiming) return;

        var visualScale = laserWidth / 600f;

        if (isFocusedAiming) {

            var laserPreview = new Sprite("laser_indicator") {
                Origin = new Vector2(0f, 0.5f),
                Position = player.Position + aimAngleVector * laserWidth / 2f,
                Rotation = aimAngle,
                Scale = new Vector2(10000f, visualScale),
                Color = new Color4(1f, 1f, 1f, 0.1f),
                UseColorSwapping = true,
            };

            var laserPreviewStart = new Sprite(laserPreview) {
                SpriteName = "laser_indicator_start",
                Origin = new Vector2(1f, 0.5f),
                Scale = new Vector2(visualScale),
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
                UseColorSwapping = true,
            };

            var laserPreviewStart = new Sprite(laserPreview) {
                SpriteName = "laser_indicator_start",
                Origin = new Vector2(1f, 0.5f),
                Scale = new Vector2(visualScale)
            };

            var line = new Rectangle() {
                Size = new Vector2((unfocusedLaserPosition - player.Position).Length - laserPositionPreview.Radius, 4f),
                Origin = new Vector2(0f, 0.5f),
                Position = player.Position,
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



    private void CalculateUnfocusedLaserPositionAndAngle(Player player) {
        var cos = MathF.Cos(aimAngle);
        var sin = MathF.Sin(aimAngle);

        var distToVerticalWall = MathF.Max(
            (player.Match.Bounds.X - player.Position.X) / cos,
            (-player.Match.Bounds.X - player.Position.X) / cos);

        var distToHorizontalWall = MathF.Max(
            (player.Match.Bounds.Y - player.Position.Y) / sin,
            (-player.Match.Bounds.Y - player.Position.Y) / sin);

        var distToWall = MathF.Min(distToVerticalWall, distToHorizontalWall);

        unfocusedLaserPosition = player.Position + new Vector2(distToWall * cos, distToWall * sin);

        var halfPI = MathF.PI / 2f;

        if (distToHorizontalWall <= distToVerticalWall) {

            unfocusedAngle = halfPI * -MathF.Sign(unfocusedLaserPosition.Y);

        } else {

            unfocusedAngle = halfPI * MathF.Sign(unfocusedLaserPosition.X) + halfPI;

        }
    }
}