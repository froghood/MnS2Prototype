using Touhou.Networking;

namespace Touhou.Objects.Characters;

public abstract class Bomb {

    public Timer CooldownTimer { get; set; }

    public abstract void LocalPress(Time cooldownOverflow, bool focused);
    public abstract void RemotePress(Packet packet);

}

public abstract class Bomb<T> : Bomb where T : Character {

    protected T c;
    public Bomb(T c) => this.c = c;
}