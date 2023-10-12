using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class Timestop : Effect {


    private Queue<TimestopProjectile> projectiles = new();
    private Time procTime = Time.InSeconds(0f);
    private bool isPlayerOwned;
    private Action cancelCallback;

    public Timestop(bool isPlayerOwned, Action cancelCallback = null) {
        this.isPlayerOwned = isPlayerOwned;
        this.cancelCallback = cancelCallback;
    }

    public override void PlayerUpdate(Player player) {
        while (LifeTime >= procTime) {

            if (player.Power >= 12) {
                player.SpendPower(12);
                procTime += Time.InSeconds(0.25f);
            } else {
                Cancel();
                break;
            }

        }
    }

    public override void OpponentUpdate(Opponent opponent) {

    }

    public override void Cancel(Time time = default) {
        if (isPlayerOwned) {
            while (projectiles.Count > 0) projectiles.Dequeue().Unfreeze(Time.InSeconds(0f), false);

            var packet = new Packet(PacketType.EffectCancelled)
            .In(EffectType.Timestop)
            .In(Game.Network.Time);

            Game.Network.Send(packet);

        } else {
            while (projectiles.Count > 0) projectiles.Dequeue().Unfreeze(time, true);
        }

        cancelCallback?.Invoke();

        base.Cancel();
    }

    public void AddProjectile(TimestopProjectile projectile) {
        projectiles.Enqueue(projectile);
    }
}