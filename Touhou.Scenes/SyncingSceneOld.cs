// using SFML.System;
// using Touhou.Net;

// namespace Touhou.Scenes;
// public class SyncingSceneOld : BaseScene {

//     private bool _hosting;
//     private Packet _syncStartPacket;
//     private Clock _retryTimer = new();

//     private PacketReceive _receivedSyncPacket;

//     private int _sendIndex = 0;
//     //private int _receiveIndex = 0;

//     private float _prevDiff;
//     private int _ourDiff;

//     public SyncingSceneOld(bool hosting, Packet syncStartPacket) {
//         _hosting = hosting;
//         _syncStartPacket = syncStartPacket;

//         //_sendIndex = _hosting ? 0 : -1;

//         _receivedSyncPacket = (packet, endPoint) => {

//             //System.Console.WriteLine(packet.Type);

//             if (packet.Type != PacketType.Sync) return;

//             int theirIndex = BitConverter.ToInt32(packet.Data.Skip(0).Take(4).ToArray());
//             int theirTime = BitConverter.ToInt32(packet.Data.Skip(4).Take(4).ToArray());
//             int theirDiff = BitConverter.ToInt32(packet.Data.Skip(8).Take(4).ToArray());



//             if (theirIndex < _sendIndex) return;

//             Console.ForegroundColor = ConsoleColor.Cyan;
//             System.Console.WriteLine($"Received Sync: {theirIndex}, {theirTime}, {theirDiff}");

//             //_receiveIndex++;
//             _sendIndex++;

//             var diffDiff = theirDiff - (Game.Network.Time - theirTime);
//             var offsetChange = (int)Math.Ceiling(diffDiff * 0.1f);
//             Game.Network.Offset += offsetChange;
//             Console.ForegroundColor = ConsoleColor.Red;
//             System.Console.WriteLine($"{diffDiff} | {offsetChange}");

//             int ourTime = Game.Network.Time;
//             _ourDiff = ourTime - theirTime;

//             //System.Console.WriteLine(_ourDiff);

//             SendSync(_sendIndex, ourTime, _ourDiff);

//         };
//     }

//     public override void Initialize() {

//         Game.Network.Received += _receivedSyncPacket;

//         System.Console.WriteLine("Syncing");
//         System.Console.WriteLine(_hosting);
//         if (_hosting) {
//             Game.Network.Send(PacketType.SyncStart,
//                 BitConverter.GetBytes(Game.Network.Time));
//             _retryTimer.Restart();
//         } else {
//             _sendIndex++;

//             var theirTime = BitConverter.ToInt32(_syncStartPacket.Data);

//             var ourTime = Game.Network.Time;
//             _ourDiff = ourTime - theirTime;

//             SendSync(_sendIndex, ourTime, _ourDiff);
//         }
//     }

//     public override void Update() {
//         if (_retryTimer.ElapsedTime.AsSeconds() > 0.1f) {
//             if (_hosting && _sendIndex == 0) {
//                 Game.Network.Send(PacketType.SyncStart, BitConverter.GetBytes(Game.Network.Time));
//             } else if (_sendIndex >= 0) {
//                 SendSync(_sendIndex, Game.Network.Time, _ourDiff);
//             }
//             _retryTimer.Restart();
//         }
//     }

//     public override void Render() {

//     }

//     private void SendSync(int index, int time, int diff) {

//         if (!_hosting) index--;

//         Console.ResetColor();
//         System.Console.WriteLine($"Sending sync {index}");
//         Game.Network.Send(PacketType.Sync,
//             BitConverter.GetBytes(index).Concat(
//             BitConverter.GetBytes(time)).Concat(
//             BitConverter.GetBytes(diff)).ToArray());
//         _retryTimer.Restart();
//     }
// }
