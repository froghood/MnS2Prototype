using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuSecondary : Attack {
    private readonly float aimRange = 80f; // degrees
    private readonly float aimStrength = 0.2f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

    private bool attackHold;
    private float normalizedAimOffset;
    private float aimOffset;


    // cooldowns
    private readonly Time primaryCooldown = Time.InSeconds(0.5f);
    private readonly Time secondaryCooldown = Time.InSeconds(2.5f);
    private readonly Time spellACooldown = Time.InSeconds(0.5f);
    private readonly Time spellBCooldown = Time.InSeconds(0.5f);



    public ReimuSecondary() {
        Holdable = true;
    }

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        player.DisableAttacks(PlayerActions.Primary, PlayerActions.SpellA, PlayerActions.SpellB);
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

        float[] angles = { -1.5f, -1f, 1f, 1.5f };
        //float[] angles = { 0f };

        foreach (var angle in angles) {
            var projectile = new LocalHomingAmulet(player.Position, player.AngleToOpponent + aimOffset + angle, 150f, 200f, 10f) {
                Color = new Color4(0, 1f, 0.8f, 0.3f),
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
        player.ApplyAttackCooldowns(spellACooldown - cooldownOverflow, PlayerActions.SpellA);
        player.ApplyAttackCooldowns(spellBCooldown - cooldownOverflow, PlayerActions.SpellB);

        player.EnableAttacks(PlayerActions.Primary, PlayerActions.SpellA, PlayerActions.SpellB);

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;
    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {
        packet.Out(out Time theirTime).Out(out Vector2 theirPosition).Out(out float theirAngle);
        var delta = Game.Network.Time - theirTime;


        float[] angles = { -1.5f, -1f, 1f, 1.5f };
        //float[] angles = { 0f };

        foreach (var angle in angles) {
            var projectile = new RemoteHomingAmulet(theirPosition, theirAngle + angle, 150f, 200f, 10f, delta) {
                Color = new Color4(1f, 0, 0.8f, 1f),
                GrazeAmount = 3,
            };

            opponent.Scene.AddEntity(projectile);
        }
    }

    public override void PlayerRender(Player player) {
        if (!attackHold) return;

        // var indicatorStates = new SpriteStates() {
        //     Origin = new Vector2(0.5f, 0.5f),
        //     Position = player.Position,
        //     Scale = new Vector2(1f, 1f) * 0.35f,
        //     Color4 = new Color4(255, 255, 255, 40),
        // };

        // var shader = new TShader("aimIndicator");
        // shader.SetUniform("angle", player.AngleToOpponent);
        // shader.SetUniform("arc", TMathF.degToRad(aimRange));

        //Game.DrawSprite("aimindicator", indicatorStates, shader, Layers.Player);

        float darkness = 1f - 0.4f * MathF.Abs(normalizedAimOffset);

        var aimArrowSprite = new Sprite("aimarrow2") {
            Origin = new Vector2(0.0625f, 0.5f),
            Position = player.Position,
            Rotation = player.AngleToOpponent + aimOffset,
            Scale = Vector2.One * 0.3f,
            Color = new Color4(1f, darkness, darkness, 0.5f),
        };

        Game.Draw(aimArrowSprite, Layers.Player);

        // var arrowStates = new SpriteStates() {
        //     Origin = new Vector2(10f, 10f),
        //     OriginType = OriginType.Position,
        //     Position = player.Position,
        //     Rotation = TMathF.radToDeg(player.AngleToOpponent + aimOffset),
        //     Scale = new Vector2(1f, 1f) * 0.35f,
        //     Color4 = new Color4(255, darkness, darkness)
        // };

        //Game.DrawSprite("aimarrow", arrowStates, Layers.Player);
    }
}