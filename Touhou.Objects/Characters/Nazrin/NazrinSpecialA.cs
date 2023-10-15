using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class NazrinSpecialA : Attack {



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        if (player is not PlayerNazrin nazrin) return;

        for (int i = 0; i < nazrin.Mice.Length; i++) {
            var mouse = nazrin.Mice[i];

            mouse.PlayerAttack((m, t) => {

                var laserRight = new Laser(
                    m.Position,
                    m.Tangent + MathF.PI / 2f,
                    20f,
                    Time.InSeconds(0.6f),
                    Time.InSeconds(0.1f),
                    true,
                    false) {
                    CanCollide = false,
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                };

                var laserLeft = new Laser(
                    m.Position,
                    m.Tangent - MathF.PI / 2f,
                    20f,
                    Time.InSeconds(0.6f),
                    Time.InSeconds(0.1f),
                    true,
                    false) {
                    CanCollide = false,
                    Color = new Color4(0f, 1f, 0f, 0.4f),
                };

                player.Scene.AddEntity(laserRight);
                laserRight.FowardTime(t);

                player.Scene.AddEntity(laserLeft);
                laserLeft.FowardTime(t);

            }, Game.Time + Time.InSeconds(0.025f) * i - cooldownOverflow);
        }
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