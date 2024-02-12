using OpenTK.Mathematics;

namespace Touhou.Objects.Projectiles;

public class SpecialAmulet : Amulet {
    public SpecialAmulet(Vector2 origin, float direction, bool isP1Owned, bool isPlayerOwned, bool isRemote) : base(origin, direction, isP1Owned, isPlayerOwned, isRemote) {
    }
}