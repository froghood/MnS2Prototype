namespace Touhou.Objects.Generics;

public class RenderCallback : Entity {
    private Action callback;

    public RenderCallback(Action callback) {
        this.callback = callback;
    }

    public override void Render() {
        callback.Invoke();
    }
}