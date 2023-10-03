namespace Touhou.Scenes;

public class SceneManager {


    public Scene Current { get => currentScene; }




    private Scene currentScene;
    private Dictionary<Type, Scene> loadedScenes = new();


    public void ChangeScene<T>(bool saveScene = false, params object[] args) where T : Scene {

        if (currentScene != null && loadedScenes.ContainsKey(currentScene.GetType())) {
            currentScene?.OnDeactivate();
        } else {
            currentScene?.OnTerminate();
        }

        var type = typeof(T);
        if (loadedScenes.TryGetValue(type, out var scene)) {

            currentScene = scene;
            currentScene?.OnReactivate();
            if (!saveScene) loadedScenes.Remove(type);

        } else {

            currentScene = (T)Activator.CreateInstance(type, args);
            currentScene?.OnInitialize();
            if (saveScene) loadedScenes.Add(type, currentScene);

        }
    }

    public void UnloadScene<T>() where T : Scene {
        var type = typeof(T);

        if (loadedScenes.TryGetValue(type, out var scene)) {
            loadedScenes.Remove(type);
        }

    }
}