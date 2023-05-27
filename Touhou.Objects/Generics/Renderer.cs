namespace Touhou.Objects.Generics;

public class Renderer : Entity {
    private Action callback;

    public Renderer(Action callback) {
        this.callback = callback;
    }

    public override void Render() {
        callback.Invoke();
    }
}