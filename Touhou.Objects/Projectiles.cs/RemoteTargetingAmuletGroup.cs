using System.Net;
using Touhou.Networking;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class RemoteTargetingAmuletGroup : Projectile {


    private List<TargetingAmulet> targetingAmulets = new();
    private readonly Time targetTime;

    public RemoteTargetingAmuletGroup(Time targetTime, bool isP1Owned, bool isPlayerOwned) : base(isP1Owned, isPlayerOwned, true) {
        this.targetTime = targetTime;
    }


    public override void Update() {

        if (LifeTime >= targetTime) {
            Destroy();

            var character = Scene.GetFirstEntityWhere<Character>(e => e.IsP1 != IsP1Owned);
            var timeOverflow = LifeTime - targetTime;

            foreach (var targetingAmulet in targetingAmulets) {

                if (targetingAmulet.IsDestroyed) continue;

                targetingAmulet.LocalTarget(character.Position, timeOverflow);
            }

            Game.NetworkOld.Send(PacketType.UpdateProjectile, Id ^ 0x80000000, Game.NetworkOld.Time - timeOverflow, character.Position);

        }
    }


    public void Add(TargetingAmulet targetingAmulet) => targetingAmulets.Add(targetingAmulet);
}