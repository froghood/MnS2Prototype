using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class SakuyaSpecialA : Attack {


    private readonly float velocity = 360f;
    private readonly float velocityDecreasePerWave = 30f;

    private readonly int waveCount = 3;
    private readonly int spreadCount = 6;
    private readonly int shotCountPerSpread = 3;
    private readonly float spreadAngle = 0.05f;

    public SakuyaSpecialA() {
        Cost = 80;
    }



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        var isTimestopped = player.GetEffect<Timestop>(out var timestop);




        for (int wave = 0; wave < waveCount; wave++) {
            for (int spread = 0; spread < spreadCount; spread++) {

                float spreadAngle = player.AngleToOpponent + MathF.Tau / spreadCount * spread + MathF.PI / spreadCount;

                for (int i = 0; i < shotCountPerSpread; i++) {

                    var angle = spreadAngle + this.spreadAngle * i - this.spreadAngle / 2f * (shotCountPerSpread - 1);


                    var projectile = new LargeKnife(player.Position, angle, velocity - velocityDecreasePerWave * wave, isTimestopped, true, false) {
                        SpawnDelay = Time.InSeconds(0.25f),
                        CanCollide = false,
                        Color = new Color4(0f, 1f, 0f, 0.4f)
                    };
                    projectile.IncreaseTime(cooldownOverflow, false);

                    timestop?.AddProjectile(projectile);

                    player.Scene.AddEntity(projectile);
                }
            }
        }

        player.SpendPower(Cost);

        player.ApplyAttackCooldowns(Time.InSeconds(0.25f) - cooldownOverflow, PlayerActions.Primary);
        player.ApplyAttackCooldowns(Time.InSeconds(0.25f) - cooldownOverflow, PlayerActions.Secondary);
        player.ApplyAttackCooldowns(Time.InSeconds(1f) - cooldownOverflow, PlayerActions.SpecialA);
        player.ApplyAttackCooldowns(Time.InSeconds(0.25f) - cooldownOverflow, PlayerActions.SpecialB);


        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.SpecialA)
        .In(Game.Network.Time - cooldownOverflow)
        .In(player.Position)
        .In(player.AngleToOpponent);

        Game.Network.Send(packet);


    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {

    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {

    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {
        packet
        .Out(out Time theirTime)
        .Out(out Vector2 theirPosition)
        .Out(out float theirAngle);

        var latency = Game.Network.Time - theirTime;

        var isTimestopped = opponent.GetEffect<Timestop>(out var timestop);

        for (int wave = 0; wave < waveCount; wave++) {
            for (int spread = 0; spread < spreadCount; spread++) {

                float spreadAngle = theirAngle + MathF.Tau / spreadCount * spread + MathF.PI / spreadCount;

                for (int i = 0; i < shotCountPerSpread; i++) {

                    var angle = spreadAngle + this.spreadAngle * i - this.spreadAngle / 2f * (shotCountPerSpread - 1);


                    var projectile = new LargeKnife(theirPosition, angle, velocity - velocityDecreasePerWave * wave, isTimestopped, false, true) {
                        SpawnDelay = Time.InSeconds(0.25f),
                        Color = new Color4(1f, 0f, 0f, 1f),
                        GrazeAmount = 3,
                    };
                    projectile.IncreaseTime(latency, true);

                    timestop?.AddProjectile(projectile);

                    opponent.Scene.AddEntity(projectile);
                }
            }
        }

    }
}