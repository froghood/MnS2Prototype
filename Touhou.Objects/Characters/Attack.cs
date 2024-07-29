using Touhou.Networking;

namespace Touhou.Objects.Characters;



public abstract class Attack {
    public bool IsFocusable { get; protected init; }
    public bool IsHoldable { get; protected init; }

    public string IconSpriteName { get; protected init; }
    public string FocusedIconSpriteName { get; protected init; }




    public virtual void LocalPress(Time overflow, bool focused) { }
    public virtual void LocalHold(Time overflow, Time holdTime, bool focused) { }
    public virtual void LocalRelease(Time overflow, Time holdTime, bool focused) { }
    public virtual void RemotePress(Packet packet) { }
    public virtual void RemoteHold(Packet packet) { }
    public virtual void RemoteRelease(Packet packet) { }
    public virtual void Render() { }
}

public abstract class Attack<T> : Attack where T : Character {


    protected T C { get; }

    public Attack(T c) => C = c;





}