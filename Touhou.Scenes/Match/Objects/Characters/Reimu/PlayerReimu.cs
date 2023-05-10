using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Scenes.Match.Objects.Characters;

public class PlayerReimu : Player {

    private float aimOffset;
    private float normalizedAimOffset;

    private bool attackHold = false;


    private AttackOld Primary;
    private AttackOld Secondary;
    private float spellAStartAngle;
    private float spellAOffsetAngle;
    private Time spellATime;
    private float spellAOffsetIncrement;

    public PlayerReimu() {
        Speed = 300f;
        FocusedSpeed = 150f;

        CreateAttack(PlayerAction.Primary, "Primary")
            .IsFocusable()
            .IsHoldable()
            .AddPress((_, _) => DisableAttacks("Secondary", "SpellA", "SpellB"))
            .AddHold(AimHold)
            .AddRelease(PrimaryRelease)
                .Build();

        CreateAttack(PlayerAction.Secondary, "Secondary")
            .IsHoldable()
            .AddPress((_, _) => DisableAttacks("Primary", "SpellA", "SpellB"))
            .AddHold(SecondaryHold)
            .AddRelease((_, _, _) => EnableAttacks("Primary", "SpellA", "SpellB"))
                .Build();

        CreateAttack(PlayerAction.SpellA, "SpellA")
            .IsHoldable()
            .AddPress(SpellAPress)
            .AddHold(SpellAHold)
            .AddRelease(SpellARelease)
                .Build();

        CreateAttack(PlayerAction.SpellB, "SpellB")
            .IsFocusable()
            .IsHoldable()
            .AddPress((_, _) => DisableAttacks("Primary", "Secondary", "SpellA"))
            .AddHold(AimHold)
            .AddRelease(SpellBRelease)
                .Build();

        AddAttack(PlayerAction.Primary, new ReimuPrimary());
        AddAttack(PlayerAction.Primary, new ReimuSecondary());
        AddAttack(PlayerAction.Primary, new ReimuSpellA());
        AddAttack(PlayerAction.Primary, new ReimuSpellB());
    }



    private void AimHold(Time cooldownOverflow, Time holdTime, bool focused) {
        float aimRange = MathF.PI / 180f * 140f;
        float aimStrength = 0.1f;
        float gamma = 1 - MathF.Pow(aimStrength, Game.Delta.AsSeconds());
        float velocityAngle = MathF.Atan2(Velocity.Y, Velocity.X);
        bool moving = (Velocity.X != 0 || Velocity.Y != 0);

        if (holdTime > Time.InMilliseconds(75)) { // 75ms / 4.5 frames
            attackHold = true;
            var arcLengthToVelocity = TMathF.NormalizeAngle(velocityAngle - TMathF.NormalizeAngle(AngleToOpponent + normalizedAimOffset * aimRange));
            if (moving) {
                normalizedAimOffset -= normalizedAimOffset * gamma;
                normalizedAimOffset += MathF.Abs(arcLengthToVelocity / aimRange) < gamma ? arcLengthToVelocity / aimRange : gamma * MathF.Sign(arcLengthToVelocity);
                //_normalizedAimOffset += MathF.Min(gamma * MathF.Sign(arcLengthToVelocity), arcLengthToVelocity / aimRange);
            } else {
                normalizedAimOffset -= normalizedAimOffset * 0.1f;
            }
        } else {
            attackHold = false;
        }

        aimOffset = normalizedAimOffset * aimRange;
    }

    private void PrimaryRelease(Time cooldownOverflow, Time holdTime, bool focused) {



        float angle = AngleToOpponent + aimOffset;

        if (focused) {
            int numShots = 5;
            float spacing = 20f;
            float speed = 350f;
            for (int index = 0; index < numShots; index++) {
                var offset = new Vector2f(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (spacing * index - spacing / 2f * (numShots - 1));
                var projectile = new LinearAmulet(Position + offset, angle, cooldownOverflow) {
                    CanCollide = false,
                    Color = new Color(0, 255, 0, 100),
                    StartingVelocity = speed * 4f,
                    GoalVelocity = speed,
                    VelocityFalloff = 0.25f,
                };
                projectile.CollisionFilters.Add(0);
                SpawnProjectile(projectile);
            }
        } else {
            int numShots = 5;
            float spacing = 0.3f;
            float speed = 150f;

            for (int index = 0; index < numShots; index++) {
                var projectile = new LinearAmulet(Position, angle + spacing * index - spacing / 2f * (numShots - 1), cooldownOverflow) {
                    CanCollide = false,
                    Color = new Color(0, 255, 0, 100),
                    StartingVelocity = speed * 4f,
                    GoalVelocity = speed,
                    VelocityFalloff = 0.25f,
                };
                projectile.CollisionFilters.Add(0);
                SpawnProjectile(projectile);
            }
        }

        ApplyCooldowns(Time.InSeconds(0.4f) - cooldownOverflow, "Primary");
        ApplyCooldowns(Time.InSeconds(0.15f) - cooldownOverflow, "Secondary", "SpellA", "SpellB");

        EnableAttacks("Secondary", "SpellA", "SpellB");

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;

        var packet = new Packet(PacketType.Primary).In(Game.Network.Time - cooldownOverflow).In(Position).In(angle).In(focused);
        Game.Network.Send(packet);
    }

    private void SecondaryHold(Time cooldownOverflow, Time heldTime, bool focused) {
        int numShots = 2;
        float angle = AngleToOpponent + aimOffset;
        float spacing = 30f;
        float speed = 350f;

        for (int index = 0; index < numShots; index++) {
            var offset = new Vector2f(MathF.Cos(angle + MathF.PI / 2f), MathF.Sin(angle + MathF.PI / 2f)) * (spacing * index - spacing / 2f * (numShots - 1));
            var projectile = new LinearAmulet(Position + offset, angle, cooldownOverflow) {
                CanCollide = false,
                Color = new Color(0, 255, 0, 100),
                StartingVelocity = speed * 4f,
                GoalVelocity = speed,
                VelocityFalloff = 0.25f,
            };
            projectile.CollisionFilters.Add(0);
            SpawnProjectile(projectile);
        }

        ApplyCooldowns(Time.InSeconds(0.08f) - cooldownOverflow, "Secondary"); // 250ms, 0.25s

        var packet = new Packet(PacketType.Secondary).In(Game.Network.Time - cooldownOverflow).In(Position).In(angle);
        Game.Network.Send(packet);
    }

    private void SpellAPress(Time cooldownOverflow, bool focused) {

        //System.Console.WriteLine("spellapress");

        spellAStartAngle = AngleToOpponent;

        System.Console.WriteLine(spellAStartAngle);

        spellAOffsetIncrement = 0f;
        spellAOffsetAngle = 0f;
        spellATime = Game.Time - cooldownOverflow;

        MovespeedModifier = 0.2f;

        DisableAttacks("Primary", "Secondary", "SpellB");

    }

    private void SpellAHold(Time cooldownOverflow, Time heldTime, bool focused) {

        int numShots = 5;
        float speed = 225f;

        while (Game.Time >= spellATime) {

            Time timeOffset = Game.Time - spellATime;
            spellATime += Time.InSeconds(0.1f);

            float angle = spellAStartAngle + spellAOffsetAngle / 360f * MathF.Tau + MathF.PI;


            for (int i = 0; i < numShots; i++) {
                var projectile = new LinearAmulet(Position, angle + MathF.Tau / numShots * i, cooldownOverflow + timeOffset) {
                    CanCollide = false,
                    Color = new Color(0, 255, 0, 100),
                    StartingVelocity = speed * 2f,
                    GoalVelocity = speed,
                    VelocityFalloff = 0.25f,
                };
                projectile.CollisionFilters.Add(0);
                SpawnProjectile(projectile);
            }

            var packet = new Packet(PacketType.SpellA).In(Game.Network.Time - cooldownOverflow + timeOffset).In(Position).In(angle);
            Game.Network.Send(packet);

            spellAOffsetIncrement += 1f;
            spellAOffsetAngle += spellAOffsetIncrement;
        }
    }

    private void SpellARelease(Time cooldownOverflow, Time heldTime, bool focused) {

        MovespeedModifier = 1f;

        ApplyCooldowns(Time.InSeconds(0.5f), "SpellA");
        ApplyCooldowns(Time.InSeconds(0.15f), "Primary", "Secondary", "SpellB");

        EnableAttacks("Primary", "Secondary", "SpellB");
    }

    private void SpellBRelease(Time cooldownOverflow, Time heldTime, bool focused) {


        float angle = AngleToOpponent + aimOffset;


        var projectile = new YinYang(Position, angle, focused ? 30f : 20f, cooldownOverflow) {
            CanCollide = false,
            Color = new Color(0, 255, 0, 100),
            Velocity = focused ? 50f : 100f,
        };

        projectile.CollisionFilters.Add(0);
        SpawnProjectile(projectile);

        ApplyCooldowns(Time.InSeconds(0.15f) - cooldownOverflow, "Primary", "Secondary", "SpellA");
        ApplyCooldowns(Time.InSeconds(1f) - cooldownOverflow, "SpellB");

        EnableAttacks("Primary", "Secondary", "SpellA");

        attackHold = false;
        aimOffset = 0f;
        normalizedAimOffset = 0f;

        var packet = new Packet(PacketType.SpellB).In(Game.Network.Time - cooldownOverflow).In(Position).In(angle).In(focused);
        Game.Network.Send(packet);
    }

    public override void Render(Time time, float delta) {
        base.Render(time, delta);
        var rect = new RectangleShape(new Vector2f(20f, 20f));
        rect.Origin = rect.Size / 2f;
        rect.Position = Position;
        rect.FillColor = attackHold ? new Color(0, 255, 200) : new Color(0, 255, 100);
        Game.Window.Draw(rect);

        if (Game.Time - InvulnerabilityTime < InvulnerabilityDuration) {
            float currentDuration = (Game.Time - InvulnerabilityTime) / (float)Game.MICROSECOND;
            byte alpha = (byte)(MathF.Floor(currentDuration * 5f) % 1f * 255f);
            rect.FillColor = new Color(255, 255, 255, alpha);
            Game.Window.Draw(rect);
        }


        int numVertices = 32;
        float aimRange = MathF.PI / 180f * 140f;
        float fullRange = aimRange * 2;
        float increment = fullRange / (numVertices - 1);

        float angleToOpponent = AngleToOpponent;

        if (attackHold) { // ~7 frames at 60fps
            var vertexArray = new VertexArray(PrimitiveType.TriangleFan);
            vertexArray.Append(new Vertex(Position, new Color(255, 255, 255, 50)));
            for (int i = 0; i < numVertices; i++) {
                vertexArray.Append(new Vertex(Position + new Vector2f(
                    MathF.Cos(angleToOpponent + aimRange - increment * i) * 40f,
                    MathF.Sin(angleToOpponent + aimRange - increment * i) * 40f
                ), new Color(255, 255, 255, 10)));
            }
            Game.Window.Draw(vertexArray);

            var shape = new RectangleShape(new Vector2f(40f, 2f));
            shape.Origin = new Vector2f(0f, 1f);
            shape.Position = Position;
            shape.Rotation = 180f / MathF.PI * (AngleToOpponent + aimOffset);
            shape.FillColor = new Color(255, (byte)MathF.Round(255f - 100f * MathF.Abs(normalizedAimOffset)), (byte)MathF.Round(255f - 100f * Math.Abs(normalizedAimOffset)));
            Game.Window.Draw(shape);
        }
    }
}