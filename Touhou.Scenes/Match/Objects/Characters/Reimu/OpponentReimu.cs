using System.Net;
using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Scenes.Match.Objects.Characters;

public class OpponentReimu : Opponent {

    public OpponentReimu(Vector2f startingPosition) : base(startingPosition) {

        AddAttack(PacketType.Primary, new ReimuPrimary());
        AddAttack(PacketType.Secondary, new ReimuSecondary());
        AddAttack(PacketType.SpellA, new ReimuSpellA());
        AddAttack(PacketType.SpellB, new ReimuSpellB());
    }

    public override void Receive(Packet packet, IPEndPoint endPoint) {
        base.Receive(packet, endPoint);

        // switch (packet.Type) {
        //     case PacketType.Primary: Primary(packet); break;
        //     case PacketType.Secondary: Secondary(packet); break;
        //     case PacketType.SpellA: SpellA(packet); break;
        //     case PacketType.SpellB: SpellB(packet); break;
        // }



    }

    public override void Render() {
        var rect = new RectangleShape(new Vector2f(20f, 20f));
        rect.Origin = rect.Size / 2f;
        rect.Position = Position;
        rect.FillColor = new Color(255, 0, 100);
        Game.Window.Draw(rect);
    }

    // protected override void Primary(Packet packet) {

    //     int numShots = 5;

    //     packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle).Out(out bool focused);
    //     Time delta = Game.Network.Time - theirTime;

    //     if (focused) {
    //         float spacing = 20f;
    //         float speed = 350f;
    //         for (int index = 0; index < numShots; index++) {
    //             var offset = new Vector2f(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (spacing * index - spacing / 2f * (numShots - 1));
    //             var projectile = new LinearAmulet(position + offset, angle) {
    //                 InterpolatedOffset = delta.AsSeconds(),

    //                 Color = new Color(255, 0, 0),

    //                 StartingVelocity = speed * 4f,
    //                 GoalVelocity = speed,
    //                 VelocityFalloff = 0.25f
    //             };
    //             projectile.CollisionFilters.Add(1);
    //             Scene.AddEntity(projectile);
    //         }
    //     } else {
    //         float spacing = 0.3f;
    //         float speed = 150f;
    //         for (int index = 0; index < numShots; index++) {
    //             var projectile = new LinearAmulet(position, angle + spacing * index - spacing / 2f * (numShots - 1)) {
    //                 InterpolatedOffset = delta.AsSeconds(),

    //                 Color = new Color(255, 0, 0),

    //                 StartingVelocity = speed * 4f,
    //                 GoalVelocity = speed,
    //                 VelocityFalloff = 0.25f
    //             };
    //             projectile.CollisionFilters.Add(1);
    //             Scene.AddEntity(projectile);
    //         }
    //     }
    // }

    // protected override void Secondary(Packet packet) {
    //     int numShots = 2;

    //     packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle);
    //     var delta = Game.Network.Time - theirTime;

    //     float spacing = 30f;
    //     float speed = 350f;

    //     for (int index = 0; index < numShots; index++) {
    //         var offset = new Vector2f(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (spacing * index - spacing / 2f * (numShots - 1));
    //         var projectile = new LinearAmulet(position + offset, angle) {
    //             InterpolatedOffset = delta.AsSeconds(),

    //             Color = new Color(255, 0, 0),

    //             StartingVelocity = speed * 4f,
    //             GoalVelocity = speed,
    //             VelocityFalloff = 0.25f
    //         };
    //         projectile.CollisionFilters.Add(1);
    //         Scene.AddEntity(projectile);
    //     }
    // }

    // protected override void SpellA(Packet packet) {
    //     int numShots = 5;
    //     float speed = 225f;

    //     packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle);
    //     var delta = Game.Network.Time - theirTime;

    //     for (int i = 0; i < numShots; i++) {
    //         var projectile = new LinearAmulet(Position, angle + MathF.Tau / numShots * i) {
    //             InterpolatedOffset = delta.AsSeconds(),

    //             Color = new Color(255, 0, 0),

    //             StartingVelocity = speed * 2f,
    //             GoalVelocity = speed,
    //             VelocityFalloff = 0.25f,
    //         };
    //         projectile.CollisionFilters.Add(1);
    //         Scene.AddEntity(projectile);
    //     }
    // }

    // protected override void SpellB(Packet packet) {

    //     packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle).Out(out bool focused);
    //     Time delta = Game.Network.Time - theirTime;

    //     var projectile = new YinYang(position, angle, focused ? 30f : 20f) {
    //         InterpolatedOffset = delta.AsSeconds(),
    //         Color = new Color(255, 0, 0),
    //         Velocity = focused ? 50f : 100f,
    //     };

    //     projectile.CollisionFilters.Add(1);
    //     Scene.AddEntity(projectile);
    // }
}