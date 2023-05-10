using Touhou.Net;

namespace Touhou.Scenes.Match.Objects.Characters;

public abstract class Attack {


    public bool Disabled { get; protected set; }
    public Time CooldownDuration { get; protected set; }
    public Time Cooldown { get; protected set; }

    public bool Focusable { get; protected init; }
    public bool Holdable { get; protected init; }

    public bool Cost { get; protected init; }

    public abstract void PlayerPress(Player player, Time cooldownOverflow, bool focused);
    public abstract void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused);
    public abstract void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused);

    public abstract void OpponentPress(Opponent opponent, Packet packet);

    public virtual void Render() { }

}