using Steamworks;

namespace Touhou.Networking;

public class SteamNetworkClient : NetworkClient {

    private SteamSocketManager socketManager;
    private SteamConnectionManager connectionManager;

    public SteamNetworkClient() {

    }



    public override void Close() {

    }

    public void Connect(SteamId id) {

    }

    public override void EndWaitingForConnection() {
        throw new NotImplementedException();
    }

    public override bool Receive(out byte[] data) {
        throw new NotImplementedException();
    }

    public override void Send(byte[] buffer) {
        throw new NotImplementedException();
    }
}