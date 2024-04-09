using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class SakuyaPrimary : Attack<Sakuya> {
    private Time heldTimeTheshold;
    private bool isActive;
    private byte fireCount;
    private float aimAngle;
    private int grazeAmount = 2;
    private readonly float velocity = 500f;
    private readonly float spacing = 80f;
    private readonly Time timeBetweenFiring = Time.InSeconds(0.32f);
    private readonly Time globalCooldown = Time.InSeconds(0.25f);





    public SakuyaPrimary(Sakuya c) : base(c) {
        IsHoldable = true;

    }

    public override void LocalPress(Time cooldownOverflow, bool focused) {

        heldTimeTheshold = Game.Time - cooldownOverflow;

        isActive = true;
        aimAngle = c.AngleToOpponent;
        fireCount = 0;

        c.DisableAttacks(
            PlayerActions.Secondary,
            PlayerActions.Special,
            PlayerActions.Super
        );
    }



    public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {



        float targetAngle = MathF.Atan2(c.Velocity.Y, c.Velocity.X);
        bool isMoving = (c.Velocity.X != 0f || c.Velocity.Y != 0f);
        float angleFromTarget = TMathF.NormalizeAngle(targetAngle - aimAngle);

        if (!c.IsFocused) {
            if (isMoving) {
                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(c.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.025f, Game.Delta.AsSeconds())));
                aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 2f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));
            } else {
                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(c.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.001f, Game.Delta.AsSeconds())));

            }
        }




        while (Game.Time >= heldTimeTheshold) {

            var timeOffset = Game.Time - heldTimeTheshold;

            heldTimeTheshold += fireCount < 2 ? Time.InSeconds(0.12f) : timeBetweenFiring;

            var angle = aimAngle;

            var numShots = (fireCount) switch {
                0 => 1,
                _ => 2
            };

            //bool isTimestopped = c.GetEffect<Timestop>(out var timestop);

            bool isTimestopped = false;

            for (int i = 0; i < numShots; i++) {
                var offset = new Vector2(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (spacing * fireCount * i - spacing * fireCount / 2f * (numShots - 1));
                var projectile = new SmallKnife(c.Position + offset, angle, velocity, isTimestopped, c.IsP1, c.IsPlayer, false) {
                    SpawnDelay = Time.InSeconds(0.15f),
                    CanCollide = false,
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                };
                if (!isTimestopped) projectile.IncreaseTime(cooldownOverflow + timeOffset, false);

                //timestop?.AddProjectile(projectile);

                c.Scene.AddEntity(projectile);
            }

            Game.NetworkOld.Send(
                PacketType.AttackReleased,
                PlayerActions.Primary,
                Game.NetworkOld.Time - cooldownOverflow + timeOffset,
                fireCount,
                c.Position,
                angle);

            fireCount = (byte)((fireCount + 1) % 3);
        }
    }



    public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
        isActive = false;

        c.EnableAttacks(
            PlayerActions.Secondary,
            PlayerActions.Special,
            PlayerActions.Super
        );

        c.ApplyAttackCooldowns(globalCooldown,
            PlayerActions.Primary,
            PlayerActions.Secondary,
            PlayerActions.Special
        );


    }



    public override void RemoteRelease(Packet packet) {

        //Log.Info("t");

        packet
        .Out(out Time theirTime)
        .Out(out byte fireCount)
        .Out(out Vector2 theirPosition)
        .Out(out float theirAngle);

        //Log.Info($"{theirTime}, {fireCount}, {theirPosition}, {theirAngle}");

        var latency = Game.NetworkOld.Time - theirTime;

        var numShots = (fireCount) switch {
            0 => 1,
            _ => 2
        };

        //var isTimestopped = c.GetEffect<Timestop>(out var timestop);

        bool isTimestopped = false;

        for (int i = 0; i < numShots; i++) {
            var offset = new Vector2(MathF.Cos(theirAngle + MathF.PI / 2f), MathF.Sin(theirAngle + MathF.PI / 2f)) * (spacing * fireCount * i - spacing * fireCount / 2f * (numShots - 1));
            var projectile = new SmallKnife(theirPosition + offset, theirAngle, velocity, isTimestopped, c.IsP1, c.IsPlayer, true) {
                SpawnDelay = Time.InSeconds(0.15f),
                Color = new Color4(1f, 0f, 0f, 1f),
                GrazeAmount = grazeAmount
            };
            projectile.IncreaseTime(latency, true);

            //if (isTimestopped) timestop.AddProjectile(projectile);

            c.Scene.AddEntity(projectile);
        }
    }

    public override void Render() {

        if (!isActive) return;

        var aimArrowSprite = new Sprite("aimarrow2") {
            Origin = new Vector2(-0.0625f, 0.5f),
            Position = c.Position,
            Rotation = aimAngle,
            Scale = new Vector2(0.3f),
            Color = new Color4(1f, 1f, 1f, 0.5f),
        };



        Game.Draw(aimArrowSprite, Layer.Player);

        base.Render();
    }
}