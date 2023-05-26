using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Scenes.Match.Objects.Characters;

public class ReimuSecondary : Attack {
    private readonly float aimRange = 80f; // degrees
    private readonly float aimStrength = 0.2f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);
    private readonly Time primaryCooldown = Time.InSeconds(1f);
    private readonly Time globalCooldown = Time.InSeconds(0.25f);
    private bool attackHold;
    private float normalizedAimOffset;
    private float aimOffset;

    public ReimuSecondary() {
        Holdable = true;
    }

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        player.DisableAttacks(PlayerAction.Primary, PlayerAction.SpellA, PlayerAction.SpellB);
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

        float[] angles = { -1f, 1f };
        //float[] angles = { 0f };

        foreach (var angle in angles) {
            var projectile = new LocalHomingAmulet(player.Position, player.AngleToOpponent + aimOffset + angle, 150f, 200f, 10f) {
                Color = new Color(0, 255, 200, 80),
                CanCollide = false,
            };

            player.Scene.AddEntity(projectile);
        }

        var packet = new Packet(PacketType.Secondary).In(Game.Network.Time).In(player.Position).In(player.AngleToOpponent + aimOffset);
        Game.Network.Send(packet);

        player.EnableAttacks(PlayerAction.Primary, PlayerAction.SpellA, PlayerAction.SpellB);

        player.ApplyCooldowns(primaryCooldown - cooldownOverflow, PlayerAction.Secondary, PlayerAction.SpellA);
        player.ApplyCooldowns(globalCooldown - cooldownOverflow, PlayerAction.Primary, PlayerAction.SpellB);

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;
    }



    public override void OpponentPress(Opponent opponent, Packet packet) {
        packet.Out(out Time theirTime, true).Out(out Vector2f theirPosition).Out(out float theirAngle);
        var delta = Game.Network.Time - theirTime;


        float[] angles = { -1f, 1f };
        //float[] angles = { 0f };

        foreach (var angle in angles) {
            var projectile = new RemoteHomingAmulet(theirPosition, theirAngle + angle, 150f, 200f, 10f) {
                Color = new Color(255, 0, 200),
                GrazeAmount = 1,
            };
            projectile.CollisionGroups.Add(1);

            opponent.Scene.AddEntity(projectile);
        }
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
        }
    }
}