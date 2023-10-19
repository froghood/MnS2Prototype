using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuSecondary : Attack {

    private readonly float velocity = 200f;
    private readonly float turnRadius = 150f;
    private readonly float hitboxRadius = 10f;
    private readonly Time spawnDuration = Time.InSeconds(0.25f);
    private readonly Time preHomingDuration = Time.InSeconds(0.5f);
    private readonly Time homingDuration = Time.InSeconds(4f);
    private readonly float[] angles = { -0.4f, -0.133f, 0.133f, 0.4f };





    private readonly float aimRange = 120f; // degrees
    private readonly float aimStrength = 0.15f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

    private bool attackHold;
    private float normalizedAimOffset;
    private float aimOffset;


    // cooldowns
    private readonly Time primaryCooldown = Time.InSeconds(0.5f);
    private readonly Time secondaryCooldown = Time.InSeconds(2.5f);
    private readonly Time specialACooldown = Time.InSeconds(0.5f);
    private readonly Time specialBCooldown = Time.InSeconds(0.5f);



    public ReimuSecondary() {
        Holdable = true;

        Icon = "reimu_secondary";
    }

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        player.DisableAttacks(PlayerActions.Primary, PlayerActions.SpecialA, PlayerActions.SpecialB);
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        float aimRangeRadians = MathF.PI / 180f * aimRange;
        float gamma = 1 - MathF.Pow(aimStrength, Game.Delta.AsSeconds());
        float velocityAngle = MathF.Atan2(player.Velocity.Y, player.Velocity.X);
        bool moving = (player.Velocity.X != 0 || player.Velocity.Y != 0);

        if (holdTime > aimHoldTimeThreshhold) { // 75ms / 4.5 frames
            attackHold = true;
            var arcLengthToVelocity = TMathF.NormalizeAngle(velocityAngle - TMathF.NormalizeAngle(player.AngleToOpponent + normalizedAimOffset * aimRangeRadians));
            if (moving) {
                normalizedAimOffset -= normalizedAimOffset * gamma;
                normalizedAimOffset += MathF.Abs(arcLengthToVelocity / aimRangeRadians) < gamma ? arcLengthToVelocity / aimRangeRadians : gamma * MathF.Sign(arcLengthToVelocity);
                //_normalizedAimOffset += MathF.Min(gamma * MathF.Sign(arcLengthToVelocity), arcLengthToVelocity / aimRange);
            } else {
                normalizedAimOffset -= normalizedAimOffset * 0.1f;
            }
        } else {
            attackHold = false;
        }

        aimOffset = normalizedAimOffset * aimRangeRadians;
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {

        foreach (var angle in angles) {
            var projectile = new LocalHomingAmulet(player.Position, player.AngleToOpponent + aimOffset + angle, turnRadius, velocity, hitboxRadius) {
                SpawnDuration = spawnDuration,
                PreHomingDuration = preHomingDuration,
                HomingDuration = homingDuration,

                Color = new Color4(0.4f, 1f, 0.667f, 0.4f),
                CanCollide = false,
            };

            player.Scene.AddEntity(projectile);
        }

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.Secondary)
        .In(Game.Network.Time)
        .In(player.Position)
        .In(player.AngleToOpponent + aimOffset);

        Game.Network.Send(packet);



        player.ApplyAttackCooldowns(primaryCooldown - cooldownOverflow, PlayerActions.Primary);
        player.ApplyAttackCooldowns(secondaryCooldown - cooldownOverflow, PlayerActions.Secondary);
        player.ApplyAttackCooldowns(specialACooldown - cooldownOverflow, PlayerActions.SpecialA);
        player.ApplyAttackCooldowns(specialBCooldown - cooldownOverflow, PlayerActions.SpecialB);

        player.EnableAttacks(PlayerActions.Primary, PlayerActions.SpecialA, PlayerActions.SpecialB);

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;
    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {
        packet.Out(out Time theirTime).Out(out Vector2 theirPosition).Out(out float theirAngle);
        var delta = Game.Network.Time - theirTime;

        foreach (var angle in angles) {
            var projectile = new RemoteHomingAmulet(theirPosition, theirAngle + angle, turnRadius, velocity, hitboxRadius) {

                SpawnDuration = spawnDuration,
                PreHomingDuration = preHomingDuration,
                HomingDuration = homingDuration,

                Color = new Color4(1f, 0.4f, 0.667f, 1f),
                GrazeAmount = 3,
            };

            opponent.Scene.AddEntity(projectile);
        }
    }

    public override void PlayerRender(Player player) {
        if (!attackHold) return;

        float darkness = 1f - 0.4f * MathF.Abs(normalizedAimOffset);

        var aimArrowSprite = new Sprite("aimarrow2") {
            Origin = new Vector2(-0.0625f, 0.5f),
            Position = player.Position,
            Rotation = player.AngleToOpponent + aimOffset,
            Scale = new Vector2(0.3f),
            Color = new Color4(1f, darkness, darkness, 0.5f),
        };

        Game.Draw(aimArrowSprite, Layer.Player);
    }
}