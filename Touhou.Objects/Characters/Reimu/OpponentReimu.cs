using System.Net;
using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects;

namespace Touhou.Objects.Characters;

public class OpponentReimu : Opponent {

    public OpponentReimu(Vector2 startingPosition) : base(startingPosition) {

        AddAttack(PlayerActions.Primary, new ReimuPrimary());
        AddAttack(PlayerActions.Secondary, new ReimuSecondary());
        AddAttack(PlayerActions.SpecialA, new ReimuSpecialA());
        AddAttack(PlayerActions.SpecialB, new ReimuSpecialB());

        AddBomb(new ReimuBomb());
    }
}