using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class MarisaSecondary : Attack {

    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);
    private readonly int grazeAmount = 6;
    private readonly int explosionGrazeAmount = 3;
    private float aimAngle;
    private bool isAiming;


    public MarisaSecondary() {
        Holdable = true;
    }



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        aimAngle = player.AngleToOpponent;

        player.DisableAttacks(
            PlayerActions.Primary,
            PlayerActions.SpecialA,
            PlayerActions.SpecialB
        );
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        if (holdTime < aimHoldTimeThreshhold) return;

        isAiming = true;

        float targetAngle = MathF.Atan2(player.Velocity.Y, player.Velocity.X);
        bool isMoving = (player.Velocity.X != 0f || player.Velocity.Y != 0f);
        float angleFromTarget = TMathF.NormalizeAngle(targetAngle - aimAngle);

        if (isMoving) {
            aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.05f, Game.Delta.AsSeconds())));
            aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 3f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));
        } else {
            aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.001f, Game.Delta.AsSeconds())));

        }

    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {

        var explodingStar = new ExplodingStar(250f, 150f, player.Position, aimAngle, true, false) {
            SpawnDuration = Time.InSeconds(0.25f),
            DestroyedOnScreenExit = false,
            CanCollide = false,
            Color = new Color4(0f, 1f, 0f, 0.4f)
        };

        player.Scene.AddEntity(explodingStar);
        explodingStar.ForwardTime(cooldownOverflow, false);

        var refundTime = Time.Min(heldTime, Time.InSeconds(0.3f));

        player.ApplyAttackCooldowns(Time.InSeconds(0.6f) - refundTime - cooldownOverflow, PlayerActions.Primary);
        player.ApplyAttackCooldowns(Time.InSeconds(1f) - refundTime - cooldownOverflow, PlayerActions.Secondary);
        player.ApplyAttackCooldowns(Time.InSeconds(0.2f),
            PlayerActions.SpecialA,
            PlayerActions.SpecialB
        );

        player.EnableAttacks(
            PlayerActions.Primary,
            PlayerActions.SpecialA,
            PlayerActions.SpecialB
        );

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.Secondary)
        .In(Game.Network.Time - cooldownOverflow)
        .In(player.Position)
        .In(aimAngle);

        Game.Network.Send(packet);

        isAiming = false;

    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {

        packet
        .Out(out Time theirTime)
        .Out(out Vector2 theirPosition)
        .Out(out float theirAngle);

        var latency = Game.Network.Time - theirTime;

        var explodingStar = new ExplodingStar(250f, 150f, theirPosition, theirAngle, false, true) {
            SpawnDuration = Time.InSeconds(0.25f),
            DestroyedOnScreenExit = false,
            Color = new Color4(1f, 0f, 0f, 1f),
            GrazeAmount = grazeAmount,
            ExplosionGrazeAmount = explosionGrazeAmount,
        };

        opponent.Scene.AddEntity(explodingStar);

        explodingStar.ForwardTime(latency, true);

    }

    public override void PlayerRender(Player player) {

        if (!isAiming) return;

        var aimArrowSprite = new Sprite("aimarrow2") {
            Origin = new Vector2(-0.0625f, 0.5f),
            Position = player.Position,
            Rotation = aimAngle,
            Scale = new Vector2(0.3f),
            Color = new Color4(1f, 1f, 1f, 0.5f),
        };

        Game.Draw(aimArrowSprite, Layer.Player);
    }
}