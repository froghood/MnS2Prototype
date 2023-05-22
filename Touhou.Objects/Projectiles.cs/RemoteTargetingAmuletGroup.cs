using System.Net;
using SFML.System;
using Touhou.Net;
using Touhou.Scenes.Match.Objects;

namespace Touhou.Objects;

public class RemoteTargetingAmuletGroup : Projectile {


    private List<TargetingAmulet> targetingAmulets = new();
    private readonly Time targetTime;

    public RemoteTargetingAmuletGroup(Time targetTime, Time spawnTimeOffset = default) : base(true, spawnTimeOffset) {
        this.targetTime = targetTime;
    }


    public override void Update() {

        var lifeTime = Game.Time - SpawnTime;

        if (lifeTime >= targetTime) {
            Destroy();

            var player = Scene.GetFirstEntity<Player>();
            var timeOverflow = lifeTime - targetTime;

            foreach (var targetingAmulet in targetingAmulets) {
                targetingAmulet.LocalTarget(player.Position, timeOverflow);
            }

            var packet = new Packet(PacketType.UpdateProjectile).In(Id ^ 0x80000000).In(Game.Network.Time - timeOverflow).In(player.Position);
            Game.Network.Send(packet);
        }
    }


    public void Add(TargetingAmulet targetingAmulet) => targetingAmulets.Add(targetingAmulet);
}