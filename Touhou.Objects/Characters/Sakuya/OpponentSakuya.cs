using OpenTK.Mathematics;

namespace Touhou.Objects.Characters;

public class OpponentSakuya : Opponent {
    public OpponentSakuya(Vector2 startingPosition) : base(startingPosition) {

        AddAttack(PlayerActions.Primary, new SakuyaPrimary());
        AddAttack(PlayerActions.Secondary, new SakuyaSecondary());
        AddAttack(PlayerActions.SpecialA, new SakuyaSpecialA());
        AddAttack(PlayerActions.SpecialB, new SakuyaSpecialB());

        AddBomb(new ReimuBomb());
    }
}