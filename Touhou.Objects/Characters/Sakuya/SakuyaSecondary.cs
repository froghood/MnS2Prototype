using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class SakuyaSecondary : Attack<Sakuya> {

    private readonly int numShots = 5;
    private readonly float spreadAngle = 0.2f;

    private readonly float deadzone = 40f;
    private readonly float velocity = 400f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

    private readonly Time cooldown = Time.InSeconds(0.75f);
    private readonly Time otherCooldown = Time.InSeconds(0.25f);

    private readonly int grazeAmount = 4;

    private float aimAngle;
    private bool isAiming;


    public SakuyaSecondary(Sakuya c) : base(c) {
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
            aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 4.5f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));
        } else {
            aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(c.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.001f, Game.Delta.AsSeconds())));

        }

    }



    public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {

        //bool isTimestopped = c.GetEffect<Timestop>(out var timestop);

        bool isTimestopped = false;


        for (int i = 0; i < numShots; i++) {

            var angle = aimAngle + spreadAngle * i - spreadAngle * (numShots - 1) / 2f;

            var angleVector = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            var offset = angleVector * deadzone;

            var projectile = new Kunai(c.Position + offset, angle, velocity, isTimestopped, c.IsP1, c.IsPlayer, false) {
                SpawnDelay = Time.InSeconds(0.25f),

                CanCollide = false,
                Color = new Color4(0f, 1f, 0f, 0.4f),
            };
            projectile.IncreaseTime(cooldownOverflow, false);

            c.Scene.AddEntity(projectile);

            //timestop?.AddProjectile(projectile);
        }




        isAiming = false;

        c.ApplyAttackCooldowns(otherCooldown - cooldownOverflow, PlayerActions.Primary);
        c.ApplyAttackCooldowns(Math.Max(cooldown - heldTime, Time.InSeconds(0.25f)) - cooldownOverflow, PlayerActions.Secondary);
        c.ApplyAttackCooldowns(otherCooldown - cooldownOverflow, PlayerActions.Special);


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
    }



    public override void RemoteRelease(Packet packet) {

        packet
        .Out(out Time theirTime)
        .Out(out Vector2 theirPosition)
        .Out(out float theirAngle);

        var latency = Game.NetworkOld.Time - theirTime;

        // var isTimestopped = c.GetEffect<Timestop>(out var timestop);

        bool isTimestopped = false;

        for (int i = 0; i < numShots; i++) {

            var angle = theirAngle + spreadAngle * i - spreadAngle * (numShots - 1) / 2f;

            var angleVector = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            var offset = angleVector * deadzone;

            var projectile = new Kunai(theirPosition + offset, angle, velocity, isTimestopped, c.IsP1, c.IsPlayer, true) {
                SpawnDelay = Time.InSeconds(0.25f),
                Color = new Color4(1f, 0f, 0f, 1f),
                GrazeAmount = grazeAmount
            };
            if (!isTimestopped) projectile.IncreaseTime(latency, true);

            // timestop?.AddProjectile(projectile);

            c.Scene.AddEntity(projectile);


        }
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