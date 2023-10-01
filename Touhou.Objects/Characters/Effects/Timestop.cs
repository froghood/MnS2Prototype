using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class Timestop : Effect {


    private Queue<TimestopProjectile> projectiles = new();
    private Time procTime = Time.InSeconds(0f);
    private bool isPlayerOwned;

    public Timestop(bool isPlayerOwned, Time duration) : base(duration) {
        this.isPlayerOwned = isPlayerOwned;
    }

    public override void PlayerUpdate(Player player) {
        while (LifeTime >= procTime) {

            if (player.Power >= 16) {
                player.SpendPower(16);
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

        base.Cancel();
    }

    public void AddProjectile(TimestopProjectile projectile) {
        projectiles.Enqueue(projectile);
    }
}