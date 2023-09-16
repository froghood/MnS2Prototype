using System.Net;
using OpenTK.Mathematics;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Objects.Characters;

public class OpponentReimu : Opponent {

    public OpponentReimu(Vector2 startingPosition) : base(startingPosition) {

        AddAttack(PlayerAction.Primary, new ReimuPrimary());
        AddAttack(PlayerAction.Secondary, new ReimuSecondary());
        AddAttack(PlayerAction.SpellA, new ReimuSpellA());
        AddAttack(PlayerAction.SpellB, new ReimuSpellB());

        AddBomb(new ReimuBomb());
    }
}