namespace Touhou.Objects.Generics;

public class Controller : Entity, IControllable {
    private Action<PlayerAction> pressCallback;
    private Action<PlayerAction> releaseCallback;

    public Controller(Action<PlayerAction> pressCallback, Action<PlayerAction> releaseCallback) {
        this.pressCallback = pressCallback;
        this.releaseCallback = releaseCallback;
    }

    public void Press(PlayerAction action) {
        pressCallback.Invoke(action);
    }

    public void Release(PlayerAction action) {
        releaseCallback.Invoke(action);
    }
}