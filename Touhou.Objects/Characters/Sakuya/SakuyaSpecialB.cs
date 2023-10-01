using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class SakuyaSpecialB : Attack {





    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        if (player.HasEffect<Timestop>()) {
            player.CancelEffect<Timestop>();
        } else {
            player.ApplyEffect(new Timestop(true, long.MaxValue));
        }


        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.SpecialB);

        Game.Network.Send(packet);

    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {

    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {

    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {



        if (opponent.HasEffect<Timestop>()) return;

        System.Console.WriteLine("timestop");

        opponent.ApplyEffect(new Timestop(false, long.MaxValue));
    }

}