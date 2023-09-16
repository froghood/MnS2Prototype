using OpenTK.Mathematics;

namespace Touhou.Objects.Projectiles;

public class SpellAmulet : Amulet {
    public SpellAmulet(Vector2 origin, float direction, bool isPlayerOwned, bool isRemote, Time spawnTimeOffset = default) : base(origin, direction, isPlayerOwned, isRemote, spawnTimeOffset) {
    }
}