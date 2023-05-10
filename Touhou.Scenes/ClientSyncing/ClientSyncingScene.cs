using Touhou.Scenes.ClientSyncing.Objects;

namespace Touhou.Scenes.ClientSyncing;
public class ClientSyncingScene : Scene {

    public override void OnInitialize() {
        var receiver = new Receiver();
        AddEntity(receiver);

        receiver.Request();
    }
}
