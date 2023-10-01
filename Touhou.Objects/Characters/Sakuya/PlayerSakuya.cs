namespace Touhou.Objects.Characters;

public class PlayerSakuya : Player {
    public PlayerSakuya(bool hosting) : base(hosting) {

        Speed = 280f;
        FocusedSpeed = 170f;

        AddAttack(PlayerActions.Primary, new SakuyaPrimary());
        AddAttack(PlayerActions.Secondary, new SakuyaSecondary());
        AddAttack(PlayerActions.SpecialA, new SakuyaSpecialA());
        AddAttack(PlayerActions.SpecialB, new SakuyaSpecialB());

        AddBomb(PlayerActions.Bomb, new ReimuBomb());

    }
}