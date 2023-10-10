using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class MarisaSpecialB : Attack {
    private readonly int cost = 80;
    private readonly Time aimHoldTimeThreshhold;
    private readonly float laserWidth = 300f;
    private readonly Time startupTime = Time.InSeconds(1f);
    private readonly Time activeTime = Time.InSeconds(1f);
    private readonly int grazeAmount = 24;
    private bool isAiming;
    private float aimAngle;
    private Vector2 aimAngleVector;

    public MarisaSpecialB() {
        Holdable = true;
        Cost = cost;
    }



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        aimAngle = player.AngleToOpponent;
        aimAngleVector = new Vector2(MathF.Cos(aimAngle), MathF.Sin(aimAngle));

        player.DisableAttacks(
            PlayerActions.Primary,
            PlayerActions.Secondary,
            PlayerActions.SpecialA
        );
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {

        if (holdTime < aimHoldTimeThreshhold) return;

        isAiming = true;

        float targetAngle = MathF.Atan2(player.Velocity.Y, player.Velocity.X);
        bool isMoving = (player.Velocity.X != 0f || player.Velocity.Y != 0f);
        float angleFromTarget = TMathF.NormalizeAngle(targetAngle - aimAngle);

        if (isMoving) {

            aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.1f, Game.Delta.AsSeconds())));
            aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 0.6f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));

        } else {

            aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.1f, Game.Delta.AsSeconds())));

        }

        aimAngleVector = new Vector2(MathF.Cos(aimAngle), MathF.Sin(aimAngle));
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        var laser = new Laser(player.Position, aimAngle, laserWidth, startupTime, activeTime, true, false) {
            Color = new Color4(0f, 1f, 0f, 0.4f),
            CanCollide = false
        };

        player.Scene.AddEntity(laser);
        laser.FowardTime(cooldownOverflow);

        player.SpendPower(cost);

        player.ApplyMovespeedModifier(0f, startupTime + activeTime);


        player.ApplyAttackCooldowns(startupTime + activeTime + Time.InSeconds(0.2f) - cooldownOverflow,
            PlayerActions.Primary,
            PlayerActions.Secondary,
            PlayerActions.SpecialA,
            PlayerActions.SpecialB
        );

        player.EnableAttacks(
            PlayerActions.Primary,
            PlayerActions.Secondary,
            PlayerActions.SpecialA
        );

        isAiming = false;

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.SpecialB)
        .In(Game.Network.Time)
        .In(player.Position)
        .In(aimAngle);

        Game.Network.Send(packet);

    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {

        packet
        .Out(out Time theirTime)
        .Out(out Vector2 theirPosition)
        .Out(out float theirAngle);

        var latency = Game.Network.Time - theirTime;

        var laser = new Laser(theirPosition, theirAngle, laserWidth, startupTime, activeTime, false, true) {
            Color = new Color4(1f, 0f, 0f, 1f),
            CanCollide = false,
            GrazeAmount = grazeAmount
        };

        opponent.Scene.AddEntity(laser);
        laser.FowardTime(latency);


    }


    public override void PlayerRender(Player player) {

        if (!isAiming) return;

        var visualScale = laserWidth / 600f;

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
    }
}
