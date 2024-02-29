using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class ReimuBomb : Bomb<Reimu> {



    private readonly int numShots = 4;

    public ReimuBomb(Reimu c) : base(c) { }

    public override void LocalPress(Time cooldownOverflow, bool focused) {

        for (int i = 0; i < numShots; i++) {

            float direction = MathF.PI / 2f * i;
            float x = MathF.Abs(MathF.Cos(direction));
            float y = MathF.Abs(MathF.Sin(direction));

            var projectile = new BombWave(c.Position * new Vector2(x, y), direction, c.IsP1, c.IsPlayer, false) {
                Velocity = 750f,
                SpawnDuration = Time.InSeconds(0.5f),
                DestroyedOnScreenExit = true,
                Color = (i % 2 == 0) ? new Color4(0.5f, 1f, 0.5f, 1f) : new Color4(0.5f, 0.5f, 1f, 1f),
            };
            projectile.ForwardTime(cooldownOverflow, false);

            c.Scene.AddEntity(projectile);

        }



        var cooldown = Time.InSeconds(1.5f) - cooldownOverflow;

        CooldownTimer = new Timer(cooldown);

        c.ApplyAttackCooldowns(cooldown, PlayerActions.Primary);
        c.ApplyAttackCooldowns(cooldown, PlayerActions.Secondary);
        c.ApplyAttackCooldowns(cooldown, PlayerActions.Special);
        c.ApplyAttackCooldowns(cooldown, PlayerActions.Super);

        c.ApplyInvulnerability(cooldown);

        c.SpendBomb();

        if (Game.Network.IsConnected) {
            var packet = new Packet(PacketType.BombPressed)
            .In(Game.Network.Time - cooldownOverflow)
            .In(c.Position);

            Game.Network.Send(packet);
        }





    }
    public override void RemotePress(Packet packet) {

        packet.Out(out Time theirTime).Out(out Vector2 position);
        Time delta = Game.Network.Time - theirTime;

        Log.Info(delta.AsSeconds());

        for (int i = 0; i < numShots; i++) {

            var projectile = new BombWave(position, MathF.PI / 2f * i, c.IsP1, c.IsPlayer, true) {
                Velocity = 750f,
                SpawnDuration = Time.InSeconds(0.5f),
                DestroyedOnScreenExit = true,
                Color = (i % 2 == 0) ? new Color4(1f, 0.5f, 0.5f, 1f) : new Color4(0.5f, 0.5f, 1f, 1f),
            };

            c.Scene.AddEntity(projectile);
        }

        c.SpendBomb();

    }


}