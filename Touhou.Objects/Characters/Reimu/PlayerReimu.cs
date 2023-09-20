using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Objects.Characters;

public class PlayerReimu : Player {

    public PlayerReimu(bool hosting) : base(hosting) {
        Speed = 300f;
        FocusedSpeed = 150f;

        AddAttack(PlayerActions.Primary, new ReimuPrimary());
        AddAttack(PlayerActions.Secondary, new ReimuSecondary());
        AddAttack(PlayerActions.SpellA, new ReimuSpellA());
        AddAttack(PlayerActions.SpellB, new ReimuSpellB());

        AddBomb(PlayerActions.Bomb, new ReimuBomb());
    }
}