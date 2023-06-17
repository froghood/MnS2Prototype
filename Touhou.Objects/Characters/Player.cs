using System.Net;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

using Touhou.Objects.Characters;
using Touhou.Scenes;

namespace Touhou.Objects.Characters;

public abstract class Player : Entity, IControllable, IReceivable {

    public float Speed { get; protected init; }
    public float FocusedSpeed { get; protected init; }

    public Vector2f Velocity { get; private set; }


    public bool Focused { get => Game.IsActionPressed(PlayerAction.Focus); }

    public bool CanMove { get; private set; } = true;

    public Time InvulnerabilityTime { get; private set; }
    public Time InvulnerabilityDuration { get; private set; }
    public Time KnockbackTime { get; private set; }
    public Vector2f KnockbackStartPosition { get; private set; }
    public Vector2f KnockbackEndPosition { get; private set; }
    public Time KnockbackDuration { get; private set; }



    // power
    public int Power { get => Math.Min(Match.TotalPowerGenerated + powerGainedFromGrazing - powerSpent, 400); }
    private int powerGainedFromGrazing;
    private int powerSpent;

    public float SmoothPower => smoothPower;
    private float smoothPower;

    // hearts

    public int HeartCount { get; private set; } = 5;

    private bool isDead;
    private Time deathTime;

    public Match Match => match is null ? match = Scene.GetFirstEntity<Match>() : match;
    private Match match;


    public Dictionary<PlayerAction, Attack> Attacks { get; } = new();

    private Shader cooldownShader;

    private Dictionary<Attack, (Time Time, bool Focused)> currentlyHeldAttacks = new();



    public Opponent Opponent => opponent is null ? opponent = Scene.GetFirstEntity<Opponent>() : opponent;
    private Opponent opponent;


    private bool hosting;
    private bool isDeathConfirmed;
    private Time deathConfirmationTime;

    public Color Color { get; set; } = new Color(0, 255, 100);


    public float AngleToOpponent {
        get {
            var positionDifference = Opponent.Position - Position;
            return MathF.Atan2(positionDifference.Y, positionDifference.X);
        }
    }


    public float MovespeedModifier { get; set; } = 1f;

    public Player(bool hosting) {

        this.hosting = hosting;

        CanCollide = true;
        CollisionType = CollisionType.Player;
        CollisionGroups.Add(0);
        Hitboxes.Add(new CircleHitbox(this, new Vector2f(0f, 0f), 0.5f, Hit));
        Hitboxes.Add(new CircleHitbox(this, new Vector2f(0f, 0f), 50f, Graze));

        cooldownShader = new Shader(null, null, "assets/shaders/cooldown.frag");
    }

    public virtual void Press(PlayerAction action) { }
    public virtual void Release(PlayerAction action) { }

    public override void Update() {

        if (isDeathConfirmed && Game.Time >= deathConfirmationTime + Time.InSeconds(2f)) {
            var matchStartTime = Game.Network.Time + Time.InSeconds(3f);

            var matchRestartPacket = new Packet(PacketType.Rematch).In(matchStartTime);
            Game.Network.Send(matchRestartPacket);

            Game.Command(() => {
                Game.Scenes.PopScene();
                Game.Scenes.PushScene<MatchScene>(hosting, matchStartTime);
            });
        }

        if (!Match.Started || isDead) return;

        UpdateKnockback();
        if (CanMove) UpdateMovement();

        Position = new Vector2f(
            Math.Clamp(Position.X, -Match.Bounds.X, Match.Bounds.X),
            Math.Clamp(Position.Y, -Match.Bounds.Y, Match.Bounds.Y)
        );

        var order = Game.Input.GetActionOrder();

        foreach (var action in order) InvokeAttackPresses(action);
        foreach (var action in order) InvokeAttackHolds(action);
    }

    private void InvokeAttackPresses(PlayerAction action) {
        if (Attacks.TryGetValue(action, out var attack)) {

            if (attack.Cooldown <= 0) attack.Cooldown = 0;
            else attack.Cooldown -= Game.Delta;

            if (Power < attack.Cost || attack.Cooldown > 0 || attack.Disabled) return;

            Time cooldownOverflow = Math.Abs(attack.Cooldown);

            // when attack is buffered
            if (Game.Input.IsActionPressBuffered(action, out var state)) {
                Game.Input.ConsumePressBuffer(action);

                bool focused = state[PlayerAction.Focus];
                System.Console.WriteLine(focused);

                attack.PlayerPress(this, cooldownOverflow, focused);
                if (attack.Holdable) currentlyHeldAttacks.Add(attack, (Game.Time - cooldownOverflow, focused));

                System.Console.WriteLine($"buffer: {action}, {attack.Cooldown}");


            }

            // when attack is held but not necessarily buffered
            else if (attack.Holdable && Game.IsActionPressed(action) && !currentlyHeldAttacks.ContainsKey(attack)) {
                bool focused = Game.IsActionPressed(PlayerAction.Focus);

                attack.PlayerPress(this, cooldownOverflow, focused);
                currentlyHeldAttacks.Add(attack, (Game.Time - cooldownOverflow, focused));

                System.Console.WriteLine($"held: {action}, {attack.Cooldown}");

            }
        }
    }

    private void InvokeAttackHolds(PlayerAction action) {
        if (Attacks.TryGetValue(action, out var attack)) {
            if (currentlyHeldAttacks.TryGetValue(attack, out var heldState)) {



                if (attack.Cooldown <= 0) attack.PlayerHold(this, Math.Abs(attack.Cooldown), Game.Time - heldState.Time, heldState.Focused);

                if (Game.Input.IsActionReleaseBuffered(action) || Power < attack.Cost) {
                    attack.PlayerRelease(this, Math.Abs(attack.Cooldown), Game.Time - heldState.Time, heldState.Focused);

                    currentlyHeldAttacks.Remove(attack);
                }
            }
        }
    }



    private void UpdateMovement() {
        var movementVector = new Vector2f(
            (Game.IsActionPressed(PlayerAction.Right) ? 1f : 0f) - (Game.IsActionPressed(PlayerAction.Left) ? 1f : 0f),
            (Game.IsActionPressed(PlayerAction.Down) ? 1f : 0f) - (Game.IsActionPressed(PlayerAction.Up) ? 1f : 0f)
        );

        float movementAngle = MathF.Atan2(movementVector.Y, movementVector.X);

        ChangeVelocity(new Vector2f(
            MathF.Abs(MathF.Cos(movementAngle)) * movementVector.X,
            MathF.Abs(MathF.Sin(movementAngle)) * movementVector.Y)
                * (Focused ? FocusedSpeed : Speed) * MovespeedModifier);

        Position += Velocity * Game.Delta.AsSeconds();


    }

    private void UpdateKnockback() {
        if (Game.Time - KnockbackTime >= KnockbackDuration) {
            CanMove = true;
            return;
        }

        var t = MathF.Min((Game.Time - KnockbackTime) / (float)KnockbackDuration, 1f);

        Position = (KnockbackEndPosition - KnockbackStartPosition) * Easing.Out(t, 5f) + KnockbackStartPosition;

    }

    public override void Render() {

        if (!isDead) {
            var states = new SpriteStates() {
                Origin = new Vector2f(0.4f, 0.7f),
                Position = Position,
                Scale = new Vector2f(MathF.Sign(Position.X - Opponent.Position.X), 1f) * 0.15f,
                Color = Color
            };

            Game.DrawSprite("reimu", states, Layers.Player);

            if (Focused) {
                states = new SpriteStates() {
                    Origin = new Vector2f(0.5f, 0.5f),
                    Position = Position,
                    Scale = new Vector2f(1f, 1f) * 0.15f,
                };

                var shader = new TShader("projectileColor");
                shader.SetUniform("color", Color);

                Game.DrawSprite("hitboxfocused", states, shader, Layers.Foreground1);
            }


        }

        foreach (var attack in Attacks.Values) {
            attack.PlayerRender(this);
        }
    }

    public void Hit(Entity entity) {
        if (Game.Time - InvulnerabilityTime < InvulnerabilityDuration || isDead) return;

        System.Console.WriteLine("collide");

        if (entity is Projectile projectile) {

            projectile.Destroy();
            // must toggle to last bit because the opponents' projectile ids are opposite
            var destroyProjectilePacket = new Packet(PacketType.DestroyProjectile).In(projectile.Id ^ 0x80000000);
            Game.Network.Send(destroyProjectilePacket);

            HeartCount--;
            if (HeartCount <= 0) {
                Die();
                return;
            }





            Scene.AddEntity(new HitExplosion(Position, 0.5f, 100f, Color));

            var angleToOpponent = AngleToOpponent + MathF.PI;

            ApplyKnockback(angleToOpponent, 100f, Time.InSeconds(1)); // 1s
            ApplyInvulnerability(Time.InSeconds(2)); // 2s

            var hitPacket = new Packet(PacketType.Hit).In(Game.Network.Time).In(Position).In(angleToOpponent);
            Game.Network.Send(hitPacket);

            Game.Sounds.Play("hit");
            if (HeartCount == 1) Game.Sounds.Play("low_hearts");

        }
    }

    private void Die() {

        isDead = true;
        deathTime = Game.Network.Time;

        Scene.AddEntity(new HitExplosion(Position, 1f, 500f, Color));

        Game.Sounds.Play("death");

        var packet = new Packet(PacketType.Death).In(deathTime).In(Position);
        Game.Network.Send(packet);
    }

    public void Graze(Entity entity) {
        if (entity is Projectile projectile) {
            if (isDead || projectile.Grazed) return;
            powerGainedFromGrazing += projectile.GrazeAmount;

            Game.Sounds.Play("graze");

            projectile.Graze();

            var packet = new Packet(PacketType.Grazed).In(projectile.GrazeAmount);
            Game.Network.Send(packet);
        }
    }

    private void ApplyKnockback(float angle, float strength, Time duration) {
        CanMove = false;
        Velocity = new Vector2f(0f, 0f);
        KnockbackTime = Game.Time;
        KnockbackStartPosition = Position;
        KnockbackEndPosition = Position + new Vector2f(strength * MathF.Cos(angle), strength * MathF.Sin(angle));
        KnockbackDuration = duration;
    }

    private void ApplyInvulnerability(Time duration) {
        InvulnerabilityTime = Game.Time;
        InvulnerabilityDuration = duration;
    }

    public void ApplyCooldowns(Time duration, params PlayerAction[] actions) {
        foreach (var action in actions) {
            if (Attacks.TryGetValue(action, out var attack) && duration > attack.Cooldown) {
                attack.CooldownDuration = duration;
                attack.Cooldown = duration;
            }
        }

    }

    public void DisableAttacks(params PlayerAction[] actions) {
        foreach (var action in actions) {
            if (Attacks.TryGetValue(action, out var attack)) attack.Disable();
        }
    }

    public void EnableAttacks(params PlayerAction[] actions) {
        foreach (var action in actions) {
            if (Attacks.TryGetValue(action, out var attack)) attack.Enable();
        }
    }

    public void SpendPower(int amount) {
        var powerOverflow = Math.Max((Match.TotalPowerGenerated + powerGainedFromGrazing - powerSpent) - 400, 0);

        int spent = powerOverflow + amount;
        powerSpent += spent;

        var packet = new Packet(PacketType.SpentPower).In(spent);
        Game.Network.Send(packet);
    }

    private void ChangeVelocity(Vector2f newVelocity) {
        if (newVelocity == Velocity) return;
        Velocity = newVelocity;
        var packet = new Packet(PacketType.VelocityChanged);
        packet.In((long)Game.Network.Time).In(Position).In(Velocity).In(Focused);
        Game.Network.Send(packet);
    }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        switch (packet.Type) {
            case PacketType.Death:

                // received death before death confirmation: resolve tie
                if (isDead) {
                    packet.Out(out Time theirDeathTime, true);

                    // send confirmation if opponent died before we did
                    if (theirDeathTime < deathTime || (theirDeathTime == deathTime && hosting)) {
                        Game.Network.Send(new Packet(PacketType.DeathConfirmation));

                        // give us a point
                    }


                } else {
                    ApplyInvulnerability(Time.InSeconds(999f));

                    Game.Network.Send(new Packet(PacketType.DeathConfirmation));

                    // give us a point
                }


                break;
            case PacketType.DeathConfirmation:

                isDeathConfirmed = true;
                deathConfirmationTime = Game.Time;



                break;
            case PacketType.Rematch:
                packet.Out(out Time startTime, true);

                Game.Command(() => {
                    Game.Scenes.PopScene();
                    Game.Scenes.PushScene<MatchScene>(hosting, startTime);
                });
                break;

        }
    }

    protected void AddAttack(PlayerAction action, Attack attack) => Attacks[action] = attack;

}