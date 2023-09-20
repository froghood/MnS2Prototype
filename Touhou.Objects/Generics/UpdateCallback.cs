namespace Touhou.Objects.Generics;

public class UpdateCallback : Entity {
    private Action callback;

    public UpdateCallback(Action callback) {
        this.callback = callback;
    }

    public override void Update() {
        callback.Invoke();
    }
}