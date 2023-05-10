using System.Net;

namespace Touhou.Net;
public delegate void PacketReceived(Packet packet, IPEndPoint ipEndPoint);