using Touhou.Networking;

namespace Touhou.Objects.Characters;




public abstract class AttackOlder {



    public bool HasFocusVariant { get; protected init; }
    public bool IsHoldable { get; protected init; }
    public int Cost { get; protected init; }
    public string Icon { get; protected init; }
    public string FocusedIcon { get; protected init; }



    public Timer CooldownTimer { get => cooldownTimer; }
    public bool IsDisabled { get => isDisabled; }



    private Timer cooldownTimer;
    private bool isDisabled;



    public void ApplyCooldown(Time duration) {
        if (cooldownTimer.Remaining < duration) cooldownTimer = new Timer(duration);
    }

    public void Enable() => isDisabled = false;
    public void Disable() => isDisabled = true;



    public abstract void LocalPress(Time cooldownOverflow, bool focused);
    public abstract void LocalHold(Time cooldownOverflow, Time holdTime, bool focused);
    public abstract void LocalRelease(Time cooldownOverflow, Time holdTIme, bool focused);
    public abstract void RemoteRelease(Packet packet);

    public virtual void Render() { }
}



public abstract class AttackOlder<T> : AttackOlder where T : Character {

    protected T c;

    public AttackOlder(T c) => this.c = c;

}