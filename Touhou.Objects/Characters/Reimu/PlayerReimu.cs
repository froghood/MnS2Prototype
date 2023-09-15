using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Objects.Characters;

public class PlayerReimu : Player {

    public PlayerReimu(bool hosting) : base(hosting) {
        Speed = 300f;
        FocusedSpeed = 150f;

        AddAttack(PlayerAction.Primary, new ReimuPrimary());
        AddAttack(PlayerAction.Secondary, new ReimuSecondary());
        AddAttack(PlayerAction.SpellA, new YukariSpellA());
        AddAttack(PlayerAction.SpellB, new ReimuSpellB());

        AddBomb(PlayerAction.Bomb, new ReimuBomb());
    }
}