using System.Net;
using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class LocalTargetingAmuletGroup : Projectile {


    private List<TargetingAmulet> targetingAmulets = new();

    public LocalTargetingAmuletGroup() : base(true, false) {
        DestroyedOnScreenExit = false;
    }


    public override void Receive(Packet packet, IPEndPoint endPoint) {

        //Log.Info("local group receive");

        if (packet.Type != PacketType.UpdateProjectile) return;

        packet.Out(out uint id, true);

        //Log.Info($"{Id}, {id}");

        if (id != Id) return;

        Destroy();

        packet.Out(out Time theirTime).Out(out Vector2 theirPosition);

        foreach (var targetingAmulet in targetingAmulets) {

            if (targetingAmulet.IsDestroyed) continue;

            targetingAmulet.RemoteTarget(theirTime, theirPosition);
        }
    }


    public void Add(TargetingAmulet targetingAmulet) => targetingAmulets.Add(targetingAmulet);




}