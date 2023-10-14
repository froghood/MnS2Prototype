using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class NazrinPrimary : Attack {



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {


        var ball = new Ball(player.Position, player.AngleToOpponent, 300f, true, false) {
            SpawnDelay = Time.InSeconds(0.25f),
            CanCollide = false,
            Color = new Color4(0f, 1f, 0f, 0.4f),
        };

        player.Scene.AddEntity(ball);
        ball.ForwardTime(cooldownOverflow, false);

        if (player is not PlayerNazrin nazrin) return;

        foreach (var mouse in nazrin.Mice) {
            var ballRight = new Ball(mouse.Position, mouse.Tangent + MathF.PI / 2f, 300f, true, false) {
                SpawnDelay = Time.InSeconds(0.25f),
                CanCollide = false,
                Color = new Color4(0f, 1f, 0f, 0.4f),
            };

            var ballLeft = new Ball(mouse.Position, mouse.Tangent - MathF.PI / 2f, 300f, true, false) {
                SpawnDelay = Time.InSeconds(0.25f),
                CanCollide = false,
                Color = new Color4(0f, 1f, 0f, 0.4f),
            };

            player.Scene.AddEntity(ballRight);
            ballRight.ForwardTime(cooldownOverflow, false);

            player.Scene.AddEntity(ballLeft);
            ballLeft.ForwardTime(cooldownOverflow, false);

            // var diff = player.OpponentPosition - mouse.Position;
            // var angle = MathF.Atan2(diff.Y, diff.X);

            // var mouseBall = new Ball(mouse.Position, angle, 300f, true, false) {
            //     SpawnDelay = Time.InSeconds(0.25f),
            //     CanCollide = false,
            //     Color = new Color4(0f, 1f, 0f, 0.4f),
            // };

            // player.Scene.AddEntity(mouseBall);
            // mouseBall.ForwardTime(cooldownOverflow, false);

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