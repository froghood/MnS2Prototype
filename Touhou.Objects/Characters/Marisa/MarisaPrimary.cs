using Touhou.Net;

namespace Touhou.Objects.Characters;

public class MarisaPrimary : Attack {


    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        throw new NotImplementedException();
    }

    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        throw new NotImplementedException();
    }

    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        throw new NotImplementedException();
    }

    public override void OpponentReleased(Opponent opponent, Packet packet) {
        throw new NotImplementedException();
    }
}