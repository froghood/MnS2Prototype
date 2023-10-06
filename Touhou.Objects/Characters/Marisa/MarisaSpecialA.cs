using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class MarisaSpecialA : Attack {

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        var projectile = new ShootingStar(300f, player.Position, player.AngleToOpponent, true, false) {
            SpawnDelay = Time.InSeconds(0.25f),
            CanCollide = false,
            Color = new Color4(0f, 1f, 0f, 0.4f),
        };
        projectile.IncreaseTime(cooldownOverflow, false);

        player.Scene.AddEntity(projectile);


        var seed = Game.Random.Next();
        var random = new Random(seed);

        for (int i = 0; i < 38; i++) {
            
        }


        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.SpecialA)
        .In(Game.Network.Time - cooldownOverflow)
        .In(player.Position)
        .In(player.AngleToOpponent)
        .In(seed);

        Game.Network.Send(packet);
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        throw new NotImplementedException();
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        throw new NotImplementedException();
    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {
        packet
        .Out(out Time theirTime)
        .Out(out Vector2 theirPosition)
        .Out(out float theirAngle)
        .Out(out int theirSeed);

        var latency = Game.Network.Time - theirTime;

        var projectile = new ShootingStar(300f, theirPosition, theirAngle, false, true) {
            SpawnDelay = Time.InSeconds(0.25f),
            Color = new Color4(1f, 0f, 0f, 1f),
            GrazeAmount = 8,
        };
        projectile.IncreaseTime(latency, true);

        opponent.Scene.AddEntity(projectile);

        var random = new Random(theirSeed);

        for (int i = 0; i < 38; i++) {
            var randomAngle = random.NextSingle() * MathF.Tau;
            var trail = new TrailStar(0.15f * (i + 1), randomAngle, 300f, 10f, theirPosition, theirAngle, false, true) {
                SpawnDelay = Time.InSeconds(0.25f),
                Color = new Color4(1f, 0f, 0f, 1f),
                GrazeAmount = 2,
            };
            trail.IncreaseTime(latency, false);

            opponent.Scene.AddEntity(trail);
        }
    }
}