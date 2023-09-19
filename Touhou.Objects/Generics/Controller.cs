namespace Touhou.Objects.Generics;

public class Controller : Entity, IControllable {
    private Action<PlayerActions> pressCallback;
    private Action<PlayerActions> releaseCallback;

    public Controller(Action<PlayerActions> pressCallback, Action<PlayerActions> releaseCallback) {
        this.pressCallback = pressCallback;
        this.releaseCallback = releaseCallback;
    }

    public void Press(PlayerActions action) {
        pressCallback.Invoke(action);
    }

    public void Release(PlayerActions action) {
        releaseCallback.Invoke(action);
    }
}