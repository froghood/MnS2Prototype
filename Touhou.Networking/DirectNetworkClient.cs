using System.Net;
using System.Net.Sockets;

namespace Touhou.Networking;

public class DirectNetworkClient : NetworkClient {


    public override bool IsConnected => isConnected;


    private UdpClient? udpClient;

    private bool isListening;
    private bool isConnected;


    public override void Send(byte[] message) {

        if (!isConnected) return;

        udpClient?.Send(message, message.Length);
    }


    public override bool Receive(out byte[] data) {

        if (udpClient?.Available > 0) {

            var endPoint = new IPEndPoint(IPAddress.Any, 0);

            data = udpClient.Receive(ref endPoint);

            if (isListening) {

                udpClient.Connect(endPoint);

                isConnected = true;
                isListening = false;
            }

            return true;
        }

        data = Array.Empty<byte>();

        return false;
    }


    public override void Close() {

        udpClient?.Close();

        isConnected = false;
        isListening = false;
    }


    public void Connect(IPEndPoint endPoint) {

        udpClient = new UdpClient();
        udpClient?.Connect(endPoint);

        isConnected = true;
    }


    public void Host(int port) {

        udpClient = new UdpClient(port);

        isListening = true;
    }
}