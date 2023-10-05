namespace Touhou.Scenes;

public class SceneManager {


    public Scene Current { get => currentScene; }




    private Scene currentScene;
    private Dictionary<Type, Scene> savedScenes = new();


    public void ChangeScene<T>(bool saveCurrent = false, params object[] args) where T : Scene {

        if (currentScene != null && saveCurrent) {
            currentScene?.OnDeactivate();
            savedScenes.TryAdd(currentScene.GetType(), currentScene);
        } else {
            currentScene?.OnTerminate();
        }

        var type = typeof(T);
        if (savedScenes.TryGetValue(type, out var savedScene)) {
            currentScene = savedScene;
            currentScene?.OnReactivate();
            savedScenes.Remove(type);
        } else {
            currentScene = (T)Activator.CreateInstance(type, args);
            currentScene?.OnInitialize();
        }

    }
    public void ChangeSceneOld<T>(bool saveCurrent = false, params object[] args) where T : Scene {

        if (currentScene != null && savedScenes.ContainsKey(currentScene.GetType())) {
            currentScene?.OnDeactivate();
        } else {
            currentScene?.OnTerminate();
        }

        var type = typeof(T);
        if (savedScenes.TryGetValue(type, out var scene)) {

            currentScene = scene;
            currentScene?.OnReactivate();
            if (!saveCurrent) savedScenes.Remove(type);

        } else {

            currentScene = (T)Activator.CreateInstance(type, args);
            currentScene?.OnInitialize();
            if (saveCurrent) savedScenes.Add(type, currentScene);

        }
    }

    public void UnloadScene<T>() where T : Scene {
        var type = typeof(T);

        if (savedScenes.TryGetValue(type, out var scene)) {
            savedScenes.Remove(type);
        }

    }
}