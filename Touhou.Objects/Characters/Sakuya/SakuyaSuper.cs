using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class SakuyaSuper : Attack<Sakuya> {

    public SakuyaSuper(Sakuya c) : base(c) {
        Cost = 12;
    }

    public override void LocalPress(Time cooldownOverflow, bool focused) {

        // if (c.HasEffect<Timestop>()) {
        //     c.CancelEffect<Timestop>();

        // } else {
        //     var vfx = new TimestopVFX(() => c.Position, Graphics.Layer.Background1);

        //     c.ApplyEffect(new Timestop(true, vfx.Destroy));

        //     c.Scene.AddEntity(vfx);

        //     var packet = new Packet(PacketType.AttackReleased),PlayerActions.Super;

        //     Game.Network.Send(packet);
        // }




    }



    public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {

    }



    public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {

    }



    public override void RemoteRelease(Packet packet) {



        // if (c.HasEffect<Timestop>()) return;

        // var vfx = new TimestopVFX(() => c.Position, Graphics.Layer.Background2);

        // c.ApplyEffect(new Timestop(false, vfx.Destroy));

        // c.Scene.AddEntity(vfx);
    }

}