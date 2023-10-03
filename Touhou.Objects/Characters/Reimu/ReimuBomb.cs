using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuBomb : Bomb {

    private readonly int numShots = 4;

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        Log.Info("t");

        for (int i = 0; i < numShots; i++) {

            float direction = MathF.PI / 2f * i;
            float x = MathF.Abs(MathF.Cos(direction));
            float y = MathF.Abs(MathF.Sin(direction));

            Log.Info($"{x}, {y}");

            var projectile = new ReimuBombWave(player.Position * new Vector2(x, y), direction, true, false) {
                Velocity = 750f,
                SpawnDelay = Time.InSeconds(0.5f),
                DestroyedOnScreenExit = true,
                Color = (i % 2 == 0) ? new Color4(0.5f, 1f, 0.5f, 1f) : new Color4(0.5f, 0.5f, 1f, 1f),
            };
            projectile.IncreaseTime(cooldownOverflow, false);

            player.Scene.AddEntity(projectile);

        }

        Cooldown = Time.InSeconds(1.5f) - cooldownOverflow;

        player.ApplyAttackCooldowns(Cooldown, PlayerActions.Primary);
        player.ApplyAttackCooldowns(Cooldown, PlayerActions.Secondary);
        player.ApplyAttackCooldowns(Cooldown, PlayerActions.SpecialA);
        player.ApplyAttackCooldowns(Cooldown, PlayerActions.SpecialB);

        var packet = new Packet(PacketType.BombPressed)
        .In(Game.Network.Time - cooldownOverflow)
        .In(player.Position);

        Game.Network.Send(packet);

        Game.Sounds.Play("spell");
        Game.Sounds.Play("bomb");

    }
    public override void OpponentPress(Opponent opponent, Packet packet) {

        packet.Out(out Time theirTime).Out(out Vector2 position);
        Time delta = Game.Network.Time - theirTime;

        Log.Info(delta.AsSeconds());

        for (int i = 0; i < numShots; i++) {

            var projectile = new ReimuBombWave(opponent.Position, MathF.PI / 2f * i, false, true) {
                Velocity = 750f,
                SpawnDelay = Time.InSeconds(0.5f),
                DestroyedOnScreenExit = true,
                Color = (i % 2 == 0) ? new Color4(1f, 0.5f, 0.5f, 1f) : new Color4(0.5f, 0.5f, 1f, 1f),
            };

            opponent.Scene.AddEntity(projectile);
        }
    }


}