using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;


public partial class Sakuya : Character {
    public class SpecialLvl1 : Attack<Sakuya> {

        public SpecialLvl1(Sakuya c) : base(c) { }

        public override void LocalPress(Time cooldownOverflow, bool focused) {

            if (C.IsTimestopped) {
                C.DisableTimestop(0L, false);
            } else {
                C.EnableTimestop(Time.InSeconds(3f));
            }

            var packet = new Packet(PacketType.AttackReleased).In(PlayerActions.Charge);
            Game.Network.Send(packet);


            // if (C.HasEffect<Timestop>()) {
            //     C.CancelEffect<Timestop>();

            // } else {
            //     var vfx = new TimestopVFX(() => C.Position, Graphics.Layer.Background1);

            //     C.ApplyEffect(new Timestop(true, vfx.Destroy));

            //     C.Scene.AddEntity(vfx);

            //     var packet = new Packet(PacketType.AttackReleased).In(PlayerActions.Super);

            //     Game.Network.Send(packet);
            // }




        }



        public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {
        }



        public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {

        }



        public override void RemoteRelease(Packet packet) {



            // if (C.HasEffect<Timestop>()) return;

            // var vfx = new TimestopVFX(() => C.Position, Graphics.Layer.Background2);

            // C.ApplyEffect(new Timestop(false, vfx.Destroy));

            // C.Scene.AddEntity(vfx);
        }

    }
}