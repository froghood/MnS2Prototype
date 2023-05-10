using System.Net;
using Touhou.Net;

namespace Touhou.Objects;

public interface IReceivable {
    void Receive(Packet packet, IPEndPoint endPoint);
}