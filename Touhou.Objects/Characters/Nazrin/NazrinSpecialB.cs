using Touhou.Networking;

namespace Touhou.Objects.Characters;

public class NazrinSpecialB : Attack {



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {


        if (player is not PlayerNazrin nazrin) return;

        nazrin.AddMouse();

        var packet = new Packet(PacketType.AttackReleased).In(PlayerActions.SpecialB);

        Game.Network.Send(packet);
    }


    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        throw new NotImplementedException();
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        throw new NotImplementedException();
    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {

        if (opponent is not OpponentNazrin nazrin) return;



        nazrin.AddMouse();

    }
}