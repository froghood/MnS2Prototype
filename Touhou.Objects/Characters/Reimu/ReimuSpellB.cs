using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuSpellB : Attack {

    // aiming
    private bool attackHold;
    private float normalizedAimOffset;
    private float aimOffset;
    private float chargeTime = 0f;
    private readonly float aimRange = 40f; // degrees
    private readonly float aimStrength = 0.1f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

    // pattern
    private readonly int grazeAmount = 20;
    private readonly Time maxChargeTime = Time.InSeconds(2f);

    private readonly float minRadius = 20f;
    private readonly float maxRadius = 60f;
    private readonly float minVelocity = 120f;
    private readonly float maxVelocity = 60f;



    // cooldowns
    private readonly Time primaryCooldown = Time.InSeconds(0.5f);
    private readonly Time secondaryCooldown = Time.InSeconds(0.5f);
    private readonly Time spellACooldown = Time.InSeconds(0.5f);
    private readonly Time spellBCooldown = Time.InSeconds(1f);



    public ReimuSpellB() {
        Holdable = true;
        //Focusable = true;
        Cost = 80;

    }



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        player.DisableAttacks(PlayerAction.Primary, PlayerAction.Secondary, PlayerAction.SpellA);

        player.MovespeedModifier = 0.1f;


    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        float aimRangeRadians = MathF.PI / 180f * aimRange;
        float gamma = 1 - MathF.Pow(aimStrength, Game.Delta.AsSeconds());
        float velocityAngle = MathF.Atan2(player.Velocity.Y, player.Velocity.X);
        bool moving = (player.Velocity.X != 0 || player.Velocity.Y != 0);

        if (holdTime > aimHoldTimeThreshhold) {
            attackHold = true;
            var arcLengthToVelocity = TMathF.NormalizeAngle(velocityAngle - TMathF.NormalizeAngle(player.AngleToOpponent + normalizedAimOffset * aimRangeRadians));
            if (moving) {
                normalizedAimOffset -= normalizedAimOffset * gamma;
                normalizedAimOffset += MathF.Abs(arcLengthToVelocity / aimRangeRadians) < gamma ? arcLengthToVelocity / aimRangeRadians : gamma * MathF.Sign(arcLengthToVelocity);
                //_normalizedAimOffset += MathF.Min(gamma * MathF.Sign(arcLengthToVelocity), arcLengthToVelocity / aimRange);
            } else {
                normalizedAimOffset -= normalizedAimOffset * 0.1f;
            }

            chargeTime = MathF.Min(chargeTime + Game.Delta.AsSeconds() / maxChargeTime.AsSeconds(), 1f);


        } else {
            attackHold = false;
        }

        aimOffset = normalizedAimOffset * aimRangeRadians;
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        float angle = player.AngleToOpponent + aimOffset;

        // var projectile = new YinYang(player.Position, angle, false, focused ? focusedSize : unfocusedSize, cooldownOverflow) {
        //     CanCollide = false,
        //     Color = new Color(0, 255, 0, 100),
        //     Velocity = focused ? focusedVelocity : unfocusedVelocity,
        // };

        var radius = minRadius + (maxRadius - minRadius) * chargeTime;
        var velocity = minVelocity + (maxVelocity - minVelocity) * chargeTime;

        var projectile = new YinYang(player.Position, angle, false, radius, cooldownOverflow) {
            CanCollide = false,
            Color = new Color(0, 255, 0, 100),
            Velocity = velocity
        };

        projectile.CollisionGroups.Add(0);
        player.Scene.AddEntity(projectile);

        player.SpendPower(Cost);

        player.ApplyCooldowns(primaryCooldown - cooldownOverflow, PlayerAction.Primary);
        player.ApplyCooldowns(secondaryCooldown - cooldownOverflow, PlayerAction.Secondary);
        player.ApplyCooldowns(spellACooldown - cooldownOverflow, PlayerAction.SpellA);
        player.ApplyCooldowns(spellBCooldown - cooldownOverflow, PlayerAction.SpellB);

        player.EnableAttacks(PlayerAction.Primary, PlayerAction.Secondary, PlayerAction.SpellA);

        player.MovespeedModifier = 1f;

        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerAction.SpellB)
        .In(Game.Network.Time - cooldownOverflow)
        .In(player.Position)
        .In(angle)
        .In(radius).In(velocity);

        Game.Network.Send(packet);

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;

        chargeTime = 0f;
    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {
        packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle).Out(out float size).Out(out float velocity);
        Time delta = Game.Network.Time - theirTime;

        var projectile = new YinYang(position, angle, true, size) {
            InterpolatedOffset = delta.AsSeconds(),
            Color = new Color(255, 0, 0),
            GrazeAmount = grazeAmount,
            Velocity = velocity,
        };

        projectile.CollisionGroups.Add(1);
        opponent.Scene.AddEntity(projectile);
    }

    public override void PlayerRender(Player player) {
        if (!attackHold) return;

        var indicatorStates = new SpriteStates() {
            Origin = new Vector2f(0.5f, 0.5f),
            Position = player.Position,
            Scale = new Vector2f(1f, 1f) * 0.35f,
            Color = new Color(255, 255, 255, 40),
        };

        var shader = new TShader("aimIndicator");
        shader.SetUniform("angle", player.AngleToOpponent);
        shader.SetUniform("arc", TMathF.degToRad(aimRange));

        Game.DrawSprite("aimindicator", indicatorStates, shader, Layers.Player);

        byte darkness = (byte)MathF.Round(255f - 100f * MathF.Abs(normalizedAimOffset));

        var arrowStates = new SpriteStates() {
            Origin = new Vector2f(10f, 10f),
            OriginType = OriginType.Position,
            Position = player.Position,
            Rotation = TMathF.radToDeg(player.AngleToOpponent + aimOffset),
            Scale = new Vector2f(1f, 1f) * 0.35f,
            Color = new Color(255, darkness, darkness)
        };

        Game.DrawSprite("aimarrow", arrowStates, Layers.Player);

        var circleStates = new CircleStates() {
            Origin = new Vector2f(0.5f, 0.5f),
            Radius = minRadius + (maxRadius - minRadius) * chargeTime,
            Position = player.Position,
            OutlineColor = new Color(255, 255, 255, 80),
            FillColor = Color.Transparent
        };

        Game.DrawCircle(circleStates, Layers.Player);

        // var circle = new CircleShape(minRadius + (maxRadius - minRadius) * chargeTime);
        // circle.Origin = new Vector2f(1f, 1f) * circle.Radius;
        // circle.Position = player.Position;
        // circle.OutlineThickness = 1f;
        // circle.OutlineColor = new Color(255, 255, 255, 80);
        // circle.FillColor = Color.Transparent;
        // Game.Draw(circle, 0);
    }
}
