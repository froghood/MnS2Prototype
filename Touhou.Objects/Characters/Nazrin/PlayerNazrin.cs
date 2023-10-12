namespace Touhou.Objects.Characters;

public class PlayerNazrin : Player {

    private LinkedList<Mouse> mice = new();


    public PlayerNazrin(bool isP1) : base(isP1) {

        Speed = 325f;
        FocusedSpeed = 125f;

        AddAttack(PlayerActions.Primary, new NazrinPrimary());
        AddAttack(PlayerActions.Secondary, new NazrinSecondary());
        AddAttack(PlayerActions.SpecialA, new NazrinSpecialA());
        AddAttack(PlayerActions.SpecialB, new NazrinSpecialB());

        AddBomb(new ReimuBomb());
    }

    public override void Render() {

        if (IsDead) return;

        base.Render();
    }

    public void AddMouse() {
        Scene.AddEntity(new Mouse(this, mice));
    }
}