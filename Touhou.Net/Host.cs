// using System.Net;
// using System.Net.Sockets;

// namespace Touhou.Net;

// public class Host {

//     public IPEndPoint Connection { get => _connection; }

//     public event PacketReceive Received;


//     private UdpClient _client;

//     private Task _receiveTask;
//     private CancellationTokenSource _receiveCTS;
//     private CancellationToken _receiveCT;

//     private IPEndPoint _connection;

//     public Host(int port) {
//         _client = new UdpClient(new IPEndPoint(IPAddress.Any, port));
//     }

//     public void BeginReceive() {

//         _receiveCTS = new CancellationTokenSource();
//         _receiveCT = _receiveCTS.Token;

//         _receiveTask = Task.Run(() => {

//             var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

//             while (!_receiveCT.IsCancellationRequested) {
//                 var data = _client.Receive(ref ipEndPoint);

//                 if (_connection != null && ipEndPoint != _connection) continue;

//                 Received.Invoke(new Packet((PacketType)data[0], data.Skip(1).ToArray()), ipEndPoint);
//             }
//         });
//     }

//     public void EndReceive() {
//         _receiveCTS.Cancel();
//         _receiveTask.Wait();
//     }

//     public void SetConnection(IPEndPoint endPoint) => _connection = endPoint;
// }

