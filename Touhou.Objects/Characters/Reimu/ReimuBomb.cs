using OpenTK.Mathematics;
using Touhou.Net;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuBomb : Bomb {

    private readonly int numShots = 4;

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        System.Console.WriteLine("t");

        for (int i = 0; i < numShots; i++) {

            var projectile = new ReimuBombWave(player.Position, MathF.PI / 2f * i, true, false, cooldownOverflow) {
                Velocity = 750f,
                SpawnDelay = Time.InSeconds(0.5f),
                DestroyedOnScreenExit = false,
                Color = (i % 2 == 0) ? new Color4(0.5f, 0.5f, 1f, 1f) : new Color4(1f, 0.5f, 0.5f, 1f),
            };

            player.Scene.AddEntity(projectile);

            Cooldown = Time.InSeconds(1f) - cooldownOverflow;
            player.ApplyAttackCooldowns(Time.InSeconds(1f) - cooldownOverflow, PlayerAction.Primary);
            player.ApplyAttackCooldowns(Time.InSeconds(1f) - cooldownOverflow, PlayerAction.Secondary);
            player.ApplyAttackCooldowns(Time.InSeconds(1f) - cooldownOverflow, PlayerAction.SpellA);
            player.ApplyAttackCooldowns(Time.InSeconds(1f) - cooldownOverflow, PlayerAction.SpellB);
        }

    }
    public override void OpponentPress(Opponent opponent, Packet packet) {
        throw new NotImplementedException();
    }


}