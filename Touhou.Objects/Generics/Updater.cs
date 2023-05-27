namespace Touhou.Objects.Generics;

public class Updater : Entity {
    private Action callback;

    public Updater(Action callback) {
        this.callback = callback;
    }

    public override void Update() {
        callback.Invoke();
    }
}