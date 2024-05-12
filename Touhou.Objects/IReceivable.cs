using System.Net;
using Touhou.Networking;

namespace Touhou.Objects;

public interface IReceivable {
    void Receive(Packet packet);
}