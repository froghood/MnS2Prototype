using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class MarisaSpecialA : Attack {

    private readonly int cost = 80;
    private readonly float velocity = 450f;
    private readonly float trailVelocity = 22f;
    private readonly int grazeAmount = 8;
    private readonly int trailGrazeAmount = 2;
    private bool isHeld;
    private float aimWeight;
    private float spawnPositionX;
    private float spawnAngle;


    public MarisaSpecialA() {
        Holdable = true;
        Cost = cost;
    }

    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {

        isHeld = true;

        aimWeight = 0f;

        player.DisableAttacks(
            PlayerActions.Primary,
            PlayerActions.Secondary,
            PlayerActions.SpecialB
        );




    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {
        var isMoving = (player.Velocity.X != 0f || player.Velocity.Y != 0f);

        if (isMoving) {
            aimWeight *= MathF.Pow(0.3f, Game.Delta.AsSeconds());
            aimWeight += player.Velocity.Normalized().X * Game.Delta.AsSeconds();

        } else {
            aimWeight *= MathF.Pow(0.1f, Game.Delta.AsSeconds());
        }

        spawnPositionX = player.OpponentPosition.X + aimWeight * 450f;
        spawnAngle = -MathF.PI / 2f + aimWeight * 1f;
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        var seed = Game.Random.Next();

        var spawnPosition = new Vector2(spawnPositionX, player.Match.Bounds.Y);

        var shootingStar = new ShootingStar(seed, velocity, trailVelocity, spawnPosition, spawnAngle, true, false) {
            SpawnDelay = Time.InSeconds(0.25f),
            CanCollide = false,
            Color = new Color4(0f, 1f, 0f, 0.4f),
        };
        player.Scene.AddEntity(shootingStar);

        shootingStar.ForwardTime(cooldownOverflow, false);



        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.SpecialA)
        .In(Game.Network.Time - cooldownOverflow)
        .In(spawnPositionX)
        .In(spawnAngle)
        .In(seed);

        Game.Network.Send(packet);

        isHeld = false;

        player.SpendPower(cost);

        player.ApplyAttackCooldowns(Time.InSeconds(0.2f) - cooldownOverflow,
            PlayerActions.Primary,
            PlayerActions.Secondary,
            PlayerActions.SpecialA,
            PlayerActions.SpecialB
        );

        player.EnableAttacks(
            PlayerActions.Primary,
            PlayerActions.Secondary,
            PlayerActions.SpecialB
        );
    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {
        packet
        .Out(out Time theirTime)
        .Out(out float theirPositionX)
        .Out(out float theirAngle)
        .Out(out int theirSeed);

        var latency = Game.Network.Time - theirTime;

        var spawnPosition = new Vector2(theirPositionX, opponent.Match.Bounds.Y);

        var shootingStar = new ShootingStar(theirSeed, velocity, trailVelocity, spawnPosition, theirAngle, false, true) {
            SpawnDelay = Time.InSeconds(0.25f),
            Color = new Color4(1f, 0f, 0f, 1f),
            GrazeAmount = grazeAmount,
            TrailGrazeAmount = trailGrazeAmount,
        };
        opponent.Scene.AddEntity(shootingStar);

        shootingStar.ForwardTime(latency, true);


    }

    public override void PlayerRender(Player player) {

        if (!isHeld) return;

        var aimArrowSprite = new Sprite("aimarrow2") {
            Origin = new Vector2(-0.0625f, 0.5f),
            Position = new Vector2(spawnPositionX, player.Match.Bounds.Y),
            Rotation = spawnAngle,
            Scale = new Vector2(0.3f),
            Color = new Color4(1f, 1f, 1f, 0.5f),
        };



        Game.Draw(aimArrowSprite, Layer.Player);
    }
}