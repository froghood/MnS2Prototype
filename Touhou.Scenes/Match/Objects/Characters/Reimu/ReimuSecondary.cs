using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Scenes.Match.Objects.Characters;

public class ReimuSecondary : Attack {



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        var packet = new Packet(PacketType.Secondary).In(Game.Network.Time).In(player.Position).In(player.AngleToOpponent);
        Game.Network.Send(packet);
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        throw new NotImplementedException();
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        throw new NotImplementedException();
    }



    public override void OpponentPress(Opponent opponent, Packet packet) {
        packet.Out(out Time theirTime, true).Out(out Vector2f theirPosition).Out(out float theirAngle);
        var delta = Game.Network.Time - theirTime;


        var projectile = new HomingAmuletBeta(theirPosition, theirAngle, true, 200f, 200f) {
            SpawnDelay = Time.InSeconds(0.15f),
            InterpolatedOffset = delta.AsSeconds(),
            Color = new Color(255, 0, 0),
            GrazeAmount = 1,
        };
        projectile.CollisionGroups.Add(1);

        opponent.Scene.AddEntity(projectile);

    }
}