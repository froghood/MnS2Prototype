using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Scenes.Match.Objects.Characters;

public class ReimuPrimary : Attack {

    private bool attackHold;
    private float normalizedAimOffset;
    private float aimOffset;

    // aiming
    private readonly float aimRange = 140f; // degrees
    private readonly float aimStrength = 0.1f;
    private readonly Time aimHoldTimeThreshhold = Time.InMilliseconds(75);

    // pattern
    private readonly int numShots = 5;

    private readonly float unfocusedSpacing = 0.3f; // radians
    private readonly float unfocusedVelocity = 150f;

    private readonly float focusedSpacing = 20f; // pixels
    private readonly float focusedVelocity = 350f;

    private readonly float velocityFalloff = 0.25f;
    private readonly float startingVelocityModifier = 4f;

    private readonly Time primaryCooldown = Time.InSeconds(0.4f);
    private readonly Time globalCooldown = Time.InSeconds(0.15f);

    public ReimuPrimary() {
        Focusable = true;
        Holdable = true;
    }

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        player.DisableAttacks(PlayerAction.Secondary, PlayerAction.SpellA, PlayerAction.SpellB);
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
        float angle = player.AngleToOpponent + aimOffset;

        if (focused) {
            for (int index = 0; index < numShots; index++) {
                var offset = new Vector2f(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (focusedSpacing * index - focusedSpacing / 2f * (numShots - 1));
                var projectile = new LinearAmulet(player.Position + offset, angle, cooldownOverflow) {
                    CanCollide = false,
                    Color = new Color(0, 255, 0, 100),
                    StartingVelocity = focusedVelocity * startingVelocityModifier,
                    GoalVelocity = focusedVelocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.CollisionFilters.Add(0);
                player.SpawnProjectile(projectile);
            }
        } else {
            for (int index = 0; index < numShots; index++) {
                var projectile = new LinearAmulet(player.Position, angle + unfocusedSpacing * index - unfocusedSpacing / 2f * (numShots - 1), cooldownOverflow) {
                    CanCollide = false,
                    Color = new Color(0, 255, 0, 100),
                    StartingVelocity = unfocusedVelocity * startingVelocityModifier,
                    GoalVelocity = unfocusedVelocity,
                    VelocityFalloff = velocityFalloff,
                };
                projectile.CollisionFilters.Add(0);
                player.SpawnProjectile(projectile);
            }
        }

        player.ApplyCooldowns(primaryCooldown - cooldownOverflow, PlayerAction.Primary);
        player.ApplyCooldowns(globalCooldown - cooldownOverflow, PlayerAction.Secondary, PlayerAction.SpellA, PlayerAction.SpellB);

        player.EnableAttacks(PlayerAction.Secondary, PlayerAction.SpellA, PlayerAction.SpellB);

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;

        var packet = new Packet(PacketType.Primary).In(Game.Network.Time - cooldownOverflow).In(player.Position).In(angle).In(focused);
        Game.Network.Send(packet);
    }

    public override void OpponentPress(Opponent opponent, Packet packet) {

        packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle).Out(out bool focused);
        Time delta = Game.Network.Time - theirTime;

        if (focused) {
            for (int index = 0; index < numShots; index++) {
                var offset = new Vector2f(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (focusedSpacing * index - focusedSpacing / 2f * (numShots - 1));
                var projectile = new LinearAmulet(position + offset, angle) {
                    InterpolatedOffset = delta.AsSeconds(),
                    Color = new Color(255, 0, 0),
                    StartingVelocity = focusedVelocity * startingVelocityModifier,
                    GoalVelocity = focusedVelocity,
                    VelocityFalloff = velocityFalloff
                };
                projectile.CollisionFilters.Add(1);
                opponent.Scene.AddEntity(projectile);
            }
        } else {
            for (int index = 0; index < numShots; index++) {
                var projectile = new LinearAmulet(position, angle + unfocusedSpacing * index - unfocusedSpacing / 2f * (numShots - 1)) {
                    InterpolatedOffset = delta.AsSeconds(),
                    Color = new Color(255, 0, 0),
                    StartingVelocity = unfocusedVelocity * startingVelocityModifier,
                    GoalVelocity = unfocusedVelocity,
                    VelocityFalloff = velocityFalloff
                };
                projectile.CollisionFilters.Add(1);
                opponent.Scene.AddEntity(projectile);
            }
        }
    }

    public override void PlayerRender(Player player) {
        int numVertices = 32;
        float aimRange = MathF.PI / 180f * 140f;
        float fullRange = aimRange * 2;
        float increment = fullRange / (numVertices - 1);

        float angleToOpponent = player.AngleToOpponent;

        if (attackHold) { // ~7 frames at 60fps
            var vertexArray = new VertexArray(PrimitiveType.TriangleFan);
            vertexArray.Append(new Vertex(player.Position, new Color(255, 255, 255, 50)));
            for (int i = 0; i < numVertices; i++) {
                vertexArray.Append(new Vertex(player.Position + new Vector2f(
                    MathF.Cos(angleToOpponent + aimRange - increment * i) * 40f,
                    MathF.Sin(angleToOpponent + aimRange - increment * i) * 40f
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