using System.Net;
using SFML.Graphics;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Scenes.HostSyncing;

namespace Touhou.Scenes.Hosting.Objects;

public class Receiver : Entity, IReceivable {
    private readonly Text text;

    public Receiver() {
        this.text = new Text("Waiting for connection...", Game.DefaultFont, 14);
    }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        if (packet.Type != PacketType.Connection || Game.Network.Connected) return;
        Game.Network.Connect(endPoint);
        Game.Scenes.PushScene<HostSyncingScene>();
    }

    public override void Update(Time time, float delta) { }

    public override void Render(Time time, float delta) {
        Game.Window.Draw(this.text);
    }

    public override void Finalize(Time time, float delta) { }
}