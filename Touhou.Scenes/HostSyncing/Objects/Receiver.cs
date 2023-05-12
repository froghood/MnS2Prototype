using System.Net;
using SFML.Graphics;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Scenes.Match;

namespace Touhou.Scenes.HostSyncing.Objects;

public class Receiver : Entity, IReceivable {
    private readonly Text text;

    public Receiver() {
        this.text = new Text("Syncing...", Game.DefaultFont, 14);
    }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        switch (packet.Type) {
            case PacketType.TimeRequest:
                //_connectionResponseFlag = true;
                packet.Out(out Time theirTime);
                var responsePacket = new Packet(PacketType.TimeResponse).In(theirTime).In(Game.Network.Time);
                //Console.WriteLine($"Received Time Request: {theirTime}");
                Game.Network.Send(responsePacket);
                break;

            case PacketType.SyncFinished:
                packet.Out(out Time gameStartTime);
                Game.Scenes.PushScene<MatchScene>(true, gameStartTime);
                break;
        }
    }

    public override void Update() { }

    public override void Render() {
        Game.Window.Draw(this.text);
    }

    public override void PostRender() { }
}