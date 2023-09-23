using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;

namespace Touhou.Networking;

public class SteamSocketManager : SocketManager {


    public override void OnConnected(Connection connection, ConnectionInfo info) {
        log("connected", info);
        base.OnConnected(connection, info);
    }

    public override void OnConnecting(Connection connection, ConnectionInfo info) {
        log("connecting", info);

        connection.Accept();

        base.OnConnecting(connection, info);
    }

    public override void OnConnectionChanged(Connection connection, ConnectionInfo info) {
        log("connection changed", info);
        base.OnConnectionChanged(connection, info);
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo info) {
        log("disconnected", info);
        base.OnDisconnected(connection, info);
    }

    public override void OnMessage(Connection inConnection, NetIdentity identity, nint data, int size, long messageNum, long recvTime, int channel) {

        foreach (var outConnection in Connected) {
            if (outConnection.Id != inConnection.Id) { //&& identity.IsSteamId) {

                // var steamIdBytes = BitConverter.GetBytes(identity.SteamId.Value);
                // var inDataBytes = new byte[size];

                // Marshal.Copy(data, inDataBytes, 0, size);

                // nint newData = Marshal.AllocHGlobal(steamIdBytes.Length + size);
                // Marshal.Copy(steamIdBytes, 0, newData, steamIdBytes.Length);
                // Marshal.Copy(inDataBytes, steamIdBytes.Length, newData, size);

                outConnection.SendMessage(data, size, SendType.Unreliable);
            }
        }

        base.OnMessage(inConnection, identity, data, size, messageNum, recvTime, channel);
    }

    private void log(string name, ConnectionInfo info) {
        System.Console.WriteLine($"[SM] {name}: {info.Identity.Address}, {info.Identity.SteamId.Value}, {info.State}, {info.EndReason}");
    }
}