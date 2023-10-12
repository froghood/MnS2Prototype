namespace Touhou.Objects.Characters;

public class PlayerNazrin : Player {
    public PlayerNazrin(bool isP1) : base(isP1) {

        Speed = 325f;
        FocusedSpeed = 125f;

        AddAttack(PlayerActions.Primary, new NazrinPrimary());
        AddAttack(PlayerActions.Primary, new NazrinSecondary());
        AddAttack(PlayerActions.Primary, new NazrinSpecialA());
        AddAttack(PlayerActions.Primary, new NazrinSpecialB());

        AddBomb(new ReimuBomb());
    }

    public override void Render() {

        if (IsDead) return;

        base.Render();
    }
}