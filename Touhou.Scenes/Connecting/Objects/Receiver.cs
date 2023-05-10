using System.Net;
using SFML.Graphics;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Scenes.ClientSyncing;

namespace Touhou.Scenes.Connecting.Objects;

public class Receiver : Entity, IReceivable {
    private readonly Text text;

    public Receiver() {
        text = new Text("Connecting...", Game.DefaultFont, 14);
    }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        if (packet.Type != PacketType.ConnectionResponse) return;
        Game.Scenes.PushScene<ClientSyncingScene>();
    }

    public override void Update(Time time, float delta) { }

    public override void Render(Time time, float delta) {
        Game.Window.Draw(text);
    }

    public override void Finalize(Time time, float delta) { }
}