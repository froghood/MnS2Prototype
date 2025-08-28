using Touhou.Objects;

namespace Touhou.Scenes;

public class CharacterSelecTScene : TScene {
    private CharacterSelector characterSelector;

    public CharacterSelecTScene(bool isP1) {
        characterSelector = new CharacterSelector(isP1);
    }

    public override void OnInitialize() {
        AddEntity(characterSelector);
    }

    public override void OnDisconnect() {
        if (Game.Get<Settings>().UseSteam) Game.Get<Network>().DisconnectSteam();
        else Game.Get<Network>().Disconnect();

        Log.Warn("Opponent disconnected");

        Game.Get<SceneManager>().ChangeScene<MainTScene>();
    }
}