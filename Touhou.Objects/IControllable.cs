using Touhou;

public interface IControllable {
    void Press(PlayerAction action);
    void Release(PlayerAction action);
}