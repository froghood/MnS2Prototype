using Touhou.Networking;

namespace Touhou.Objects.Characters;

public abstract class Attack {


    public bool Disabled { get; private set; }

    public Timer CooldownTimer { get; set; }

    public bool Focusable { get; protected init; }
    public bool Holdable { get; protected init; }

    public int Cost { get; protected set; }

    public string Icon { get; protected set; }
    public string FocusedIcon { get; protected set; }

    public abstract void PlayerPress(Player player, Time cooldownOverflow, bool focused);
    public abstract void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused);
    public abstract void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused);

    public abstract void OpponentReleased(Opponent opponent, Packet packet);

    public virtual void PlayerRender(Player player) { }
    public virtual void OpponentRender(Opponent opponent) { }

    public void Enable() => Disabled = false;
    public void Disable() => Disabled = true;

}