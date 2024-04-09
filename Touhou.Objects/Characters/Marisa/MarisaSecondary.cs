using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class MarisaSecondary : Attack<Marisa> {

    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);
    private readonly int grazeAmount = 6;
    private readonly int explosionGrazeAmount = 3;
    private float aimAngle;
    private bool isAiming;


    public MarisaSecondary(Marisa c) : base(c) {
        IsHoldable = true;
    }



    public override void LocalPress(Time cooldownOverflow, bool focused) {

        aimAngle = c.AngleToOpponent;

        c.DisableAttacks(
            PlayerActions.Primary,
            PlayerActions.Special,
            PlayerActions.Super
        );
    }



    public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {
        if (holdTime < aimHoldTimeThreshhold) return;

        isAiming = true;

        float targetAngle = MathF.Atan2(c.Velocity.Y, c.Velocity.X);
        bool isMoving = (c.Velocity.X != 0f || c.Velocity.Y != 0f);
        float angleFromTarget = TMathF.NormalizeAngle(targetAngle - aimAngle);

        if (isMoving) {
            aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(c.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.05f, Game.Delta.AsSeconds())));
            aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 3f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));
        } else {
            aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(c.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.001f, Game.Delta.AsSeconds())));

        }

    }



    public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {

        var explodingStar = new ExplodingStar(250f, 150f, c.Position, aimAngle, c.IsP1, c.IsPlayer, false) {
            SpawnDuration = Time.InSeconds(0.25f),
            DestroyedOnScreenExit = false,
            CanCollide = false,
            Color = new Color4(0f, 1f, 0f, 0.4f)
        };

        c.Scene.AddEntity(explodingStar);
        explodingStar.ForwardTime(cooldownOverflow, false);

        var refundTime = Time.Min(heldTime, Time.InSeconds(0.3f));

        c.ApplyAttackCooldowns(Time.InSeconds(0.6f) - refundTime - cooldownOverflow, PlayerActions.Primary);
        c.ApplyAttackCooldowns(Time.InSeconds(1f) - refundTime - cooldownOverflow, PlayerActions.Secondary);
        c.ApplyAttackCooldowns(Time.InSeconds(0.2f),
            PlayerActions.Special,
            PlayerActions.Super
        );

        c.EnableAttacks(
            PlayerActions.Primary,
            PlayerActions.Special,
            PlayerActions.Super
        );

        Game.NetworkOld.Send(
            PacketType.AttackReleased,
            PlayerActions.Secondary,
            Game.NetworkOld.Time - cooldownOverflow,
            c.Position,
            aimAngle);

        isAiming = false;

    }



    public override void RemoteRelease(Packet packet) {

        packet
        .Out(out Time theirTime)
        .Out(out Vector2 theirPosition)
        .Out(out float theirAngle);

        var latency = Game.NetworkOld.Time - theirTime;

        var explodingStar = new ExplodingStar(250f, 150f, theirPosition, theirAngle, c.IsP1, c.IsPlayer, true) {
            SpawnDuration = Time.InSeconds(0.25f),
            DestroyedOnScreenExit = false,
            Color = new Color4(1f, 0f, 0f, 1f),
            GrazeAmount = grazeAmount,
            ExplosionGrazeAmount = explosionGrazeAmount,
        };

        c.Scene.AddEntity(explodingStar);

        explodingStar.ForwardTime(latency, true);

    }

    public override void Render() {

        if (!isAiming) return;

        var aimArrowSprite = new Sprite("aimarrow2") {
            Origin = new Vector2(-0.0625f, 0.5f),
            Position = c.Position,
            Rotation = aimAngle,
            Scale = new Vector2(0.3f),
            Color = new Color4(1f, 1f, 1f, 0.5f),
        };

        Game.Draw(aimArrowSprite, Layer.Player);
    }
}