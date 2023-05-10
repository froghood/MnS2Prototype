namespace Touhou.Scenes;

public class SceneManager {

    public Scene Current { get => (_scenes.Count > 0) ? _scenes.Peek() : null; }

    private Stack<Scene> _scenes = new();

    public void PushScene<T>(params object[] args) where T : Scene {
        var scene = (T)Activator.CreateInstance(typeof(T), args);
        Current?.OnDeactivate();
        _scenes.Push(scene);
        Current.OnInitialize();
    }

    public void PopScene() {
        Current.OnTerminate();
        _scenes.Pop();
        Current.OnReactivate();
    }
}