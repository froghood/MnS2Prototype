using System.Net;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;

namespace Touhou.Networking;

public class SteamConnectionManager : ConnectionManager {

    public event Action Disconnected;
    public event Action<byte[], IPEndPoint> DataReceived;

    public override void OnConnected(ConnectionInfo info) {

        log("connected", info);
        base.OnConnected(info);
    }

    public override void OnConnecting(ConnectionInfo info) {


        log("connecting", info);
        base.OnConnecting(info);
    }

    public override void OnConnectionChanged(ConnectionInfo info) {
        log("connection changed", info);
        base.OnConnectionChanged(info);
    }

    public override void OnDisconnected(ConnectionInfo info) {
        log("disconnected", info);
        Disconnected?.Invoke();

        base.OnDisconnected(info);
    }

    public override void OnMessage(nint data, int size, long messageNum, long recvTime, int channel) {

        var bytes = new byte[size];
        Marshal.Copy(data, bytes, 0, size);

        DataReceived?.Invoke(bytes, new IPEndPoint(IPAddress.Any, 0));

        base.OnMessage(data, size, messageNum, recvTime, channel);
    }

    private void log(string name, ConnectionInfo info) {
        Log.Info($"[CM] {name}: {info.Identity.Address}, {info.Identity.SteamId.Value}, {info.State}, {info.EndReason}");
    }
}