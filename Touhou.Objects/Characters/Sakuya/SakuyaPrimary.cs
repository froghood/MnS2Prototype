using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class SakuyaPrimary : Attack {
    private Time heldTimeTheshold;
    private bool isActive;
    private byte fireCount;
    private float aimAngle;
    private int grazeAmount = 2;
    private readonly float velocity = 500f;
    private readonly float spacing = 80f;
    private readonly Time timeBetweenFiring = Time.InSeconds(0.32f);
    private readonly Time globalCooldown = Time.InSeconds(0.25f);





    public SakuyaPrimary() {
        Holdable = true;
        Focusable = true;
    }

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        heldTimeTheshold = Game.Time - cooldownOverflow;

        isActive = true;
        aimAngle = player.AngleToOpponent;
        fireCount = 0;

        player.DisableAttacks(
            PlayerActions.Secondary,
            PlayerActions.SpecialA,
            PlayerActions.SpecialB
        );
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {



        float targetAngle = MathF.Atan2(player.Velocity.Y, player.Velocity.X);
        bool isMoving = (player.Velocity.X != 0f || player.Velocity.Y != 0f);
        float angleFromTarget = TMathF.NormalizeAngle(targetAngle - aimAngle);

        if (!player.Focused) {
            if (isMoving) {
                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.025f, Game.Delta.AsSeconds())));
                aimAngle = TMathF.NormalizeAngle(aimAngle + MathF.Min(MathF.Abs(angleFromTarget), 2f * Game.Delta.AsSeconds()) * MathF.Sign(angleFromTarget));
            } else {
                aimAngle = TMathF.NormalizeAngle(aimAngle + TMathF.NormalizeAngle(player.AngleToOpponent - aimAngle) * (1f - MathF.Pow(0.001f, Game.Delta.AsSeconds())));

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

            bool isTimestopped = player.GetEffect<Timestop>(out var timestop);

            for (int i = 0; i < numShots; i++) {
                var offset = new Vector2(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (spacing * fireCount * i - spacing * fireCount / 2f * (numShots - 1));
                var projectile = new SmallKnife(player.Position + offset, angle, velocity, isTimestopped, true, false) {
                    SpawnDelay = Time.InSeconds(0.15f),
                    CanCollide = false,
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                };
                if (!isTimestopped) projectile.IncreaseTime(cooldownOverflow + timeOffset, false);

                timestop?.AddProjectile(projectile);

                player.Scene.AddEntity(projectile);
            }



            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.Primary)
            .In(Game.Network.Time - cooldownOverflow + timeOffset)
            .In((byte)fireCount)
            .In(player.Position)
            .In(angle);

            fireCount = (byte)((fireCount + 1) % 3);

            Game.Network.Send(packet);

        }
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        isActive = false;

        player.EnableAttacks(
            PlayerActions.Secondary,
            PlayerActions.SpecialA,
            PlayerActions.SpecialB
        );

        player.ApplyAttackCooldowns(globalCooldown,
            PlayerActions.Primary,
            PlayerActions.Secondary,
            PlayerActions.SpecialA
        );


    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {

        //System.Console.WriteLine("t");

        packet
        .Out(out Time theirTime)
        .Out(out byte fireCount)
        .Out(out Vector2 theirPosition)
        .Out(out float theirAngle);

        //System.Console.WriteLine($"{theirTime}, {fireCount}, {theirPosition}, {theirAngle}");

        var latency = Game.Network.Time - theirTime;

        var numShots = (fireCount) switch {
            0 => 1,
            _ => 2
        };

        var isTimestopped = opponent.GetEffect<Timestop>(out var timestop);

        for (int i = 0; i < numShots; i++) {
            var offset = new Vector2(MathF.Cos(theirAngle + MathF.PI / 2f), MathF.Sin(theirAngle + MathF.PI / 2f)) * (spacing * fireCount * i - spacing * fireCount / 2f * (numShots - 1));
            var projectile = new SmallKnife(theirPosition + offset, theirAngle, velocity, isTimestopped, false, true) {
                SpawnDelay = Time.InSeconds(0.15f),
                Color = new Color4(1f, 0f, 0f, 1f),
                GrazeAmount = grazeAmount
            };
            projectile.IncreaseTime(latency, true);

            if (isTimestopped) timestop.AddProjectile(projectile);

            opponent.Scene.AddEntity(projectile);
        }
    }

    public override void PlayerRender(Player player) {

        if (!isActive) return;

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