using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Scenes.Match.Objects.Characters;

public class ReimuSpellB : Attack {

    private bool attackHold;
    private float normalizedAimOffset;
    private float aimOffset;
    private float sizeCharge = 20f;
    private float velocityCharge = 150f;

    // aiming
    private readonly float aimRange = 30f; // degrees
    private readonly float aimStrength = 0.1f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

    // pattern
    private readonly int grazeAmount = 20;
    private readonly float unfocusedVelocity = 120f;
    private readonly float unfocusedSize = 30f;

    private readonly float focusedVelocity = 40f;
    private readonly float focusedSize = 40f;

    private readonly Time globalCooldown = Time.InSeconds(0.25f);
    private readonly Time spellCooldown = Time.InSeconds(0.5f);

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

            sizeCharge = MathF.Min(sizeCharge + Game.Delta.AsSeconds() * 15f, 50f);
            velocityCharge = MathF.Max(velocityCharge - Game.Delta.AsSeconds() * 45f, 50f);


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

        var projectile = new YinYang(player.Position, angle, false, sizeCharge, cooldownOverflow) {
            CanCollide = false,
            Color = new Color(0, 255, 0, 100),
            Velocity = velocityCharge
        };

        projectile.CollisionGroups.Add(0);
        player.Scene.AddEntity(projectile);

        player.SpendPower(Cost);

        player.ApplyCooldowns(globalCooldown - cooldownOverflow, PlayerAction.Primary, PlayerAction.Secondary, PlayerAction.SpellA);
        player.ApplyCooldowns(spellCooldown - cooldownOverflow, PlayerAction.SpellB);

        player.EnableAttacks(PlayerAction.Primary, PlayerAction.Secondary, PlayerAction.SpellA);



        player.MovespeedModifier = 1f;

        //var packet = new Packet(PacketType.SpellB).In(Game.Network.Time - cooldownOverflow).In(player.Position).In(angle).In(focused);
        var packet = new Packet(PacketType.SpellB).In(Game.Network.Time - cooldownOverflow).In(player.Position).In(angle).In(sizeCharge).In(velocityCharge);
        Game.Network.Send(packet);

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;
        sizeCharge = 20f;
        velocityCharge = 140f;
    }



    public override void OpponentPress(Opponent opponent, Packet packet) {
        //packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle).Out(out bool focused);
        packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle).Out(out float size).Out(out float velocity);
        Time delta = Game.Network.Time - theirTime;

        // var projectile = new YinYang(position, angle, true, focused ? focusedSize : unfocusedSize) {
        //     InterpolatedOffset = delta.AsSeconds(),
        //     Color = new Color(255, 0, 0),
        //     GrazeAmount = grazeAmount,
        //     Velocity = focused ? focusedVelocity : unfocusedVelocity,
        // };

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
        int numVertices = 32;
        float aimRangeInRads = MathF.PI / 180f * aimRange;
        float fullRange = aimRangeInRads * 2;
        float increment = fullRange / (numVertices - 1);

        float angleToOpponent = player.AngleToOpponent;

        if (attackHold) { // ~7 frames at 60fps
            var vertexArray = new VertexArray(PrimitiveType.TriangleFan);
            vertexArray.Append(new Vertex(player.Position, new Color(255, 255, 255, 50)));
            for (int i = 0; i < numVertices; i++) {
                vertexArray.Append(new Vertex(player.Position + new Vector2f(
                    MathF.Cos(angleToOpponent + aimRangeInRads - increment * i) * 40f,
                    MathF.Sin(angleToOpponent + aimRangeInRads - increment * i) * 40f
                ), new Color(255, 255, 255, 10)));
            }
            Game.Window.Draw(vertexArray);

            var shape = new RectangleShape(new Vector2f(40f, 2f));
            shape.Origin = new Vector2f(0f, 1f);
            shape.Position = player.Position;
            shape.Rotation = 180f / MathF.PI * (player.AngleToOpponent + aimOffset);
            shape.FillColor = new Color(255, (byte)MathF.Round(255f - 100f * MathF.Abs(normalizedAimOffset)), (byte)MathF.Round(255f - 100f * Math.Abs(normalizedAimOffset)));
            Game.Window.Draw(shape);

            var circle = new CircleShape(sizeCharge);
            circle.Origin = new Vector2f(1f, 1f) * circle.Radius;
            circle.Position = player.Position;
            circle.OutlineThickness = 1f;
            circle.OutlineColor = new Color(255, 255, 255, 80);
            circle.FillColor = Color.Transparent;
            Game.Window.Draw(circle);
        }
    }
}