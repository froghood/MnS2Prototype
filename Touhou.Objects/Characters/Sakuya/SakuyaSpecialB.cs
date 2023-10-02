using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class SakuyaSpecialB : Attack {

    public SakuyaSpecialB() {
        Cost = 12;
    }

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        if (player.HasEffect<Timestop>()) {
            player.CancelEffect<Timestop>();

        } else {
            var vfx = new TimestopVFX(() => player.Position, Graphics.Layers.Background1);

            player.ApplyEffect(new Timestop(true, long.MaxValue, vfx.Destroy));

            player.Scene.AddEntity(vfx);

            var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.SpecialB);

            Game.Network.Send(packet);
        }




    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {

    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {

    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {



        if (opponent.HasEffect<Timestop>()) return;

        var vfx = new TimestopVFX(() => opponent.Position, Graphics.Layers.Background2);

        opponent.ApplyEffect(new Timestop(false, long.MaxValue, vfx.Destroy));

        opponent.Scene.AddEntity(vfx);
    }

}