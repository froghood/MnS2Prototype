using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class SakuyaSecondary : Attack {

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


    public SakuyaSecondary() {
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
            aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.025f, Game.Delta.AsSeconds())));
            aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 4.5f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));
        } else {
            aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.001f, Game.Delta.AsSeconds())));

        }

    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {

        bool isTimestopped = player.GetEffect<Timestop>(out var timestop);


        for (int i = 0; i < numShots; i++) {

            var angle = aimAngle + spreadAngle * i - spreadAngle * (numShots - 1) / 2f;

            var angleVector = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            var offset = angleVector * deadzone;

            var projectile = new Kunai(player.Position + offset, angle, velocity, isTimestopped, true, false) {
                SpawnDelay = Time.InSeconds(0.25f),

                CanCollide = false,
                Color = new Color4(0f, 1f, 0f, 0.4f),
            };
            projectile.IncreaseTime(cooldownOverflow, false);

            player.Scene.AddEntity(projectile);

            timestop?.AddProjectile(projectile);
        }




        isAiming = false;

        player.ApplyAttackCooldowns(otherCooldown - cooldownOverflow, PlayerActions.Primary);
        player.ApplyAttackCooldowns(Math.Max(cooldown - heldTime, Time.InSeconds(0.25f)) - cooldownOverflow, PlayerActions.Secondary);
        player.ApplyAttackCooldowns(otherCooldown - cooldownOverflow, PlayerActions.SpecialA);


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
    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {

        packet
        .Out(out Time theirTime)
        .Out(out Vector2 theirPosition)
        .Out(out float theirAngle);

        var latency = Game.Network.Time - theirTime;

        var isTimestopped = opponent.GetEffect<Timestop>(out var timestop);

        for (int i = 0; i < numShots; i++) {

            var angle = theirAngle + spreadAngle * i - spreadAngle * (numShots - 1) / 2f;

            var angleVector = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            var offset = angleVector * deadzone;

            System.Console.WriteLine(offset);

            var projectile = new Kunai(theirPosition + offset, angle, velocity, isTimestopped, false, true) {
                SpawnDelay = Time.InSeconds(0.25f),
                Color = new Color4(1f, 0f, 0f, 1f),
                GrazeAmount = grazeAmount
            };
            if (!isTimestopped) projectile.IncreaseTime(latency, true);

            timestop?.AddProjectile(projectile);

            opponent.Scene.AddEntity(projectile);


        }
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

        Game.Draw(aimArrowSprite, Layers.Player);

        base.PlayerRender(player);
    }
}