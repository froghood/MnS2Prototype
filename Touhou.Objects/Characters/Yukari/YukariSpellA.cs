using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class YukariSpellA : Attack {

    private float startingAngle;

    private float angleOffset;
    private float angleOffsetVelocity;

    private Time timeThreshold;

    // pattern
    private readonly int grazeAmount = 5;
    private readonly Time rateOfFire = Time.InSeconds(0.1f);
    private readonly float angleOffsetAcceleration = 1f;

    private readonly int numShots = 5;
    private readonly float velocity = 300f;

    private readonly float startingVelocityModifier = 2f;
    private readonly float velocityFalloff = 0.25f;

    private readonly Time spellCooldown = Time.InSeconds(1f);
    private readonly Time globalCooldown = Time.InSeconds(0.25f);

    public YukariSpellA() {
        Holdable = true;
        Cost = 8;
    }



    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {
        startingAngle = player.AngleToOpponent;

        System.Console.WriteLine(startingAngle);

        angleOffsetVelocity = 0f;
        angleOffset = 0f;
        timeThreshold = Game.Time - cooldownOverflow;

        player.MovespeedModifier = 0.2f;

        player.DisableAttacks(PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.SpellB);
    }



    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {

        while (Game.Time >= timeThreshold) {

            Time timeOffset = Game.Time - timeThreshold;
            timeThreshold += rateOfFire;

            float angle = startingAngle + angleOffset / 360f * MathF.Tau + MathF.PI;

            for (int i = 0; i < numShots; i++) {
                var projectile = new Amulet(player.Position, angle + MathF.Tau / numShots * i, true, false, cooldownOverflow + timeOffset) {
                    CanCollide = false,
                    Color = new Color4(0, 1f, 0, 0.4f),
                    StartingVelocity = velocity * startingVelocityModifier,
                    GoalVelocity = velocity,
                    VelocityFalloff = velocityFalloff,
                };
                player.Scene.AddEntity(projectile);

            }
            player.SpendPower(Cost);

            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.SpellA)
            .In(Game.Network.Time - cooldownOverflow + timeOffset)
            .In(player.Position)
            .In(angle);

            Game.Network.Send(packet);

            angleOffsetVelocity += angleOffsetAcceleration;
            angleOffset += angleOffsetVelocity;
        }
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {
        player.MovespeedModifier = 1f;

        player.ApplyAttackCooldowns(spellCooldown, PlayerActions.SpellA);
        player.ApplyAttackCooldowns(globalCooldown, PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.SpellB);

        player.EnableAttacks(PlayerActions.Primary, PlayerActions.Secondary, PlayerActions.SpellB);
    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {
        packet.Out(out Time theirTime).Out(out Vector2 position).Out(out float angle);
        var delta = Game.Network.Time - theirTime;

        for (int i = 0; i < numShots; i++) {
            var projectile = new Amulet(position, angle + MathF.Tau / numShots * i, false, true) {
                InterpolatedOffset = delta.AsSeconds(),

                Color = new Color4(1f, 0f, 0f, 1f),
                GrazeAmount = grazeAmount,
                StartingVelocity = velocity * startingVelocityModifier,
                GoalVelocity = velocity,
                VelocityFalloff = velocityFalloff,
            };
            opponent.Scene.AddEntity(projectile);
        }
    }
}