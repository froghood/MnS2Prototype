using System.Net;
using Touhou.Networking;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class RemoteTargetingAmuletGroup : Projectile {


    private List<TargetingAmulet> targetingAmulets = new();
    private readonly Time targetTime;

    public RemoteTargetingAmuletGroup(Time targetTime) : base(false, true) {
        this.targetTime = targetTime;
    }


    public override void Update() {

        if (LifeTime >= targetTime) {
            Destroy();

            var player = Scene.GetFirstEntity<Player>();
            var timeOverflow = LifeTime - targetTime;

            foreach (var targetingAmulet in targetingAmulets) {

                if (targetingAmulet.IsDestroyed) continue;

                targetingAmulet.LocalTarget(player.Position, timeOverflow);
            }

            var packet = new Packet(PacketType.UpdateProjectile).In(Id ^ 0x80000000).In(Game.Network.Time - timeOverflow).In(player.Position);
            Game.Network.Send(packet);
        }
    }


    public void Add(TargetingAmulet targetingAmulet) => targetingAmulets.Add(targetingAmulet);
}