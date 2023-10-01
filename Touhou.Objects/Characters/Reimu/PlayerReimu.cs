using Touhou.Networking;
using Touhou.Objects;

namespace Touhou.Objects.Characters;

public class PlayerReimu : Player {

    public PlayerReimu(bool hosting) : base(hosting) {
        Speed = 300f;
        FocusedSpeed = 150f;

        AddAttack(PlayerActions.Primary, new ReimuPrimary());
        AddAttack(PlayerActions.Secondary, new ReimuSecondary());
        AddAttack(PlayerActions.SpecialA, new ReimuSpecialA());
        AddAttack(PlayerActions.SpecialB, new ReimuSpecialB());

        AddBomb(PlayerActions.Bomb, new ReimuBomb());
    }
}