using Touhou.Objects;

namespace Touhou.Scenes;

public class CharacterSelectScene : Scene {
    private CharacterSelector characterSelector;

    public CharacterSelectScene(bool isP1) {
        characterSelector = new CharacterSelector(isP1);
    }

    public override void OnInitialize() {
        AddEntity(characterSelector);
    }

    public override void OnDisconnect() {
        if (Game.Settings.UseSteam) Game.NetworkOld.DisconnectSteam();
        else Game.NetworkOld.Disconnect();

        Log.Warn("Opponent disconnected");

        Game.Scenes.ChangeScene<MainScene>();
    }
}