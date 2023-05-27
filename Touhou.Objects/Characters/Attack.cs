using Touhou.Net;

namespace Touhou.Objects.Characters;

public abstract class Attack {


    public bool Disabled { get; private set; }
    public Time CooldownDuration { get; set; }
    public Time Cooldown { get; set; }

    public bool Focusable { get; protected init; }
    public bool Holdable { get; protected init; }

    public int Cost { get; protected init; }

    public abstract void PlayerPress(Player player, Time cooldownOverflow, bool focused);
    public abstract void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused);
    public abstract void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused);

    public abstract void OpponentPress(Opponent opponent, Packet packet);

    public virtual void PlayerRender(Player player) { }
    public virtual void OpponentRender(Opponent opponent) { }

    public void Enable() => Disabled = false;
    public void Disable() => Disabled = true;

}