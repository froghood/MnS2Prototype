namespace Touhou.Scenes;

public class SceneManager : GameSystem {


    public TScene Current { get => currenTScene; }




    private TScene currenTScene;
    private Dictionary<Type, TScene> savedTScenes = new();


    public void ChangeScene<T>(bool saveCurrent = false, params object[] args) where T : TScene {

        if (currenTScene != null && saveCurrent) {
            currenTScene?.OnDeactivate();
            savedTScenes.TryAdd(currenTScene.GetType(), currenTScene);
        } else {
            currenTScene?.OnTerminate();
        }

        var type = typeof(T);
        if (savedTScenes.TryGetValue(type, out var savedTScene)) {
            currenTScene = savedTScene;
            currenTScene?.OnReactivate();
            savedTScenes.Remove(type);
        } else {
            currenTScene = (T)Activator.CreateInstance(type, args);
            currenTScene?.OnInitialize();
        }

    }
    public void ChangeTSceneOld<T>(bool saveCurrent = false, params object[] args) where T : TScene {

        if (currenTScene != null && savedTScenes.ContainsKey(currenTScene.GetType())) {
            currenTScene?.OnDeactivate();
        } else {
            currenTScene?.OnTerminate();
        }

        var type = typeof(T);
        if (savedTScenes.TryGetValue(type, out var TScene)) {

            currenTScene = TScene;
            currenTScene?.OnReactivate();
            if (!saveCurrent) savedTScenes.Remove(type);

        } else {

            currenTScene = (T)Activator.CreateInstance(type, args);
            currenTScene?.OnInitialize();
            if (saveCurrent) savedTScenes.Add(type, currenTScene);

        }
    }

    public void UnloadTScene<T>() where T : TScene {
        var type = typeof(T);

        if (savedTScenes.TryGetValue(type, out var TScene)) {
            savedTScenes.Remove(type);
        }

    }
}