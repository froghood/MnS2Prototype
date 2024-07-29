using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public partial class Reimu : Character {
    private class PrimaryLvl2 : Attack<Reimu> {
        private readonly int numShots = 30;
        private readonly float velocity = 500;
        private readonly float deceleration = 500;
        private readonly Time spawnDuration = Time.InSeconds(0.15f);
        private readonly int grazeAmount = 1;

        public PrimaryLvl2(Reimu c) : base(c) {
            IsHoldable = true;
        }

        public override void LocalPress(Time cooldownOverflow, bool focused) {


            var localGroup = new LocalTargetingAmuletGroup(C.IsP1, C.IsPlayer);
            C.Scene.AddEntity(localGroup);


            var arcAngle = MathF.Tau / numShots;

            var angle = C.AngleToOpponent + arcAngle / 2f;


            for (int i = 0; i < numShots; i++) {

                var projectile = new TargetingAmulet(C.Position, angle + arcAngle * i, C.IsP1, C.IsPlayer, false, velocity, deceleration) {
                    SpawnDuration = spawnDuration,
                    DestroyedOnScreenExit = false,
                    CanCollide = false,
                    Color = new Color4(0f, 1f, 0, 0.4f),
                };
                projectile.ForwardTime(cooldownOverflow, false);

                localGroup.Add(projectile);
                C.Scene.AddEntity(projectile);
            }

            C.ApplyAbilityLock(Time.InSeconds(1f), PlayerActions.Special);
            C.ApplyAbilityLock(Time.InSeconds(0.25f), PlayerActions.Primary, PlayerActions.Secondary);

            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.Special)
            .In(Game.Network.Time - cooldownOverflow)
            .In(C.Position)
            .In(angle);

            Game.Network.Send(packet);
        }



        public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {
        }



        public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
        }


        public override void RemoteRelease(Packet packet) {

            packet.Out(out Time time).Out(out Vector2 position).Out(out float angle);
            var latency = Game.Network.Time - time;


            var remoteGroup = new RemoteTargetingAmuletGroup(Time.InSeconds(1.5f), C.IsP1, C.IsPlayer);
            C.Scene.AddEntity(remoteGroup);


            var arcAngle = MathF.Tau / numShots;

            for (int i = 0; i < numShots; i++) {
                var projectile = new TargetingAmulet(position, angle + arcAngle * i, C.IsP1, C.IsPlayer, true, velocity, deceleration) {
                    SpawnDuration = spawnDuration,
                    DestroyedOnScreenExit = false,
                    Color = new Color4(1f, 0, 0, 1f),
                    GrazeAmount = grazeAmount
                };
                projectile.ForwardTime(latency, true);

                remoteGroup.Add(projectile);
                C.Scene.AddEntity(projectile);
            }
        }
    }
}