using System.Net;
using SFML.System;
using Touhou.Net;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class LocalTargetingAmuletGroup : Projectile {


    private List<TargetingAmulet> targetingAmulets = new();

    public LocalTargetingAmuletGroup() : base(false) {
        DestroyedOnScreenExit = false;
    }


    public override void Receive(Packet packet, IPEndPoint endPoint) {

        System.Console.WriteLine("local group receive");

        if (packet.Type != PacketType.UpdateProjectile) return;

        packet.Out(out uint id, true);

        System.Console.WriteLine($"{Id}, {id}");

        if (id != Id) return;

        Destroy();

        packet.Out(out Time theirTime).Out(out Vector2f theirPosition);

        foreach (var targetingAmulet in targetingAmulets) {
            targetingAmulet.RemoteTarget(theirTime, theirPosition);
        }
    }


    public void Add(TargetingAmulet targetingAmulet) => targetingAmulets.Add(targetingAmulet);




}