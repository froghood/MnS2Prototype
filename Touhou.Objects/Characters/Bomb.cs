using Touhou.Networking;

namespace Touhou.Objects.Characters;

public abstract class Bomb {
    public Time Cooldown { get; set; }

    public abstract void PlayerPress(Player player, Time cooldownOverflow, bool focused);

    public abstract void OpponentPress(Opponent opponent, Packet packet);

    public virtual void PlayerRender(Player player) { }
    public virtual void OpponentRender(Opponent opponent) { }

}