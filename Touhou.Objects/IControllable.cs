using Touhou;

public interface IControllable {
    void Press(PlayerActions action);
    void Release(PlayerActions action);
}