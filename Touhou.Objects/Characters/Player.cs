using System.Net;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Projectiles;

using Touhou.Objects.Characters;
using Touhou.Scenes;
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public abstract class Player : Entity, IReceivable {

    public float Speed { get; protected init; }
    public float FocusedSpeed { get; protected init; }

    public Vector2 Velocity { get; private set; }
    public Vector2 MovementAngleVector { get => movementAngleVector; }



    public bool Focused { get => Game.IsActionPressed(PlayerActions.Focus); }

    public bool CanMove { get; private set; } = true;
    public bool CanAttack { get; private set; } = true;
    public Time InvulnerabilityTime { get; private set; }
    public Time InvulnerabilityDuration { get; private set; }

    public bool IsDead { get => isDead; }
    public Time DeathTime { get => deathTime; }

    private bool isKnockbacked;
    private Time knockbackTime;
    private Time knockbackDuration;
    private Vector2 knockbackStartPosition;
    private Vector2 knockbackEndPosition;




    public IEnumerable<KeyValuePair<PlayerActions, Attack>> Attacks { get => attacks.AsEnumerable(); }


    // power
    public int Power { get => Math.Min(Match.TotalPowerGenerated + powerGainedFromGrazing - powerSpent, 400); }
    private int powerGainedFromGrazing;
    private int powerSpent;

    public float SmoothPower => smoothPower;
    private float smoothPower;

    // heartss

    public int HeartCount { get; private set; } = 5;

    private bool isDead;
    private Time deathTime;

    public bool CanBomb { get; private set; } = true;
    public int BombCount { get; set; } = 3;

    public Match Match => match is null ? match = Scene.GetFirstEntity<Match>() : match;
    private Match match;


    private Dictionary<PlayerActions, Attack> attacks = new();
    public (PlayerActions Action, Bomb Bomb) Bomb { get; private set; }


    private Dictionary<Attack, (Time Time, bool Focused)> currentlyHeldAttacks = new();



    protected Opponent Opponent => opponent is null ? opponent = Scene.GetFirstEntity<Opponent>() : opponent;
    private Opponent opponent;


    private bool isP1;
    private bool isDeathConfirmed;
    private Time deathConfirmationTime;



    private Sprite characterSprite;
    private Sprite hitboxSprite;



    private bool isHit;
    private Time hitTime;
    private Time hitDuration;
    private Dictionary<Type, Effect> effects = new();
    private Vector2 movementAngleVector;
    private Time movespeedModifierTime;
    private Time movespeedModifierDuration;
    private float movespeedModifier;

    public Color4 Color { get; set; } = new Color4(0.7f, 1f, 0.7f, 1f);


    public float AngleToOpponent {
        get {
            var positionDifference = Opponent.Position - Position;
            return MathF.Atan2(positionDifference.Y, positionDifference.X);
        }
    }

    public Vector2 OpponentPosition => Opponent.Position;


    public float MovespeedModifier { get; set; } = 1f;


    public Player(bool isP1) {

        this.isP1 = isP1;

        Position = new Vector2(isP1 ? -200 : 200, 0f);

        CanCollide = true;
        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), 0.5f, CollisionGroup.Player, Hit));
        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), 50f, CollisionGroup.Player, Graze));

        //cooldownShader = new Shader(null, null, "assets/shaders/cooldown.frag");
        characterSprite = new Sprite("sakuya") {
            Origin = new Vector2(0.45f, 0.35f),
            Color = Color,
            UseColorSwapping = false,

        };

        hitboxSprite = new Sprite("hitboxfocused") {
            Origin = new Vector2(0.5f, 0.5f),
            Scale = new Vector2(1f, 1f) * 0.15f,
            UseColorSwapping = true,
        };
    }


    public override void Update() {

        if (isDeathConfirmed && Game.Time >= deathConfirmationTime + Time.InSeconds(2f)) {
            var matchStartTime = Game.Network.Time + Time.InSeconds(3f);

            var matchRestartPacket = new Packet(PacketType.Rematch).In(matchStartTime);
            Game.Network.Send(matchRestartPacket);

            Game.Command(() => {
                //Game.Scenes.PopScene();
                Game.Scenes.ChangeScene<MatchScene>(false, isP1, matchStartTime);
            });
        }

        if (!Match.Started || isDead) return;

        UpdateEffects();
        UpdateHit();
        UpdateKnockback();
        UpdateMovement();

        Position = new Vector2(
            Math.Clamp(Position.X, -Match.Bounds.X, Match.Bounds.X),
            Math.Clamp(Position.Y, -Match.Bounds.Y, Match.Bounds.Y)
        );

        var order = Game.Input.GetActionOrder();
        foreach (var action in order) InvokeActionPresses(action);
        foreach (var action in order) InvokeActionHolds(action);
    }



    private void UpdateEffects() {

        var toRemove = new List<Type>();

        foreach (var (type, effect) in effects) {
            if (effect.HasTimedOut || effect.IsCanceled) {
                toRemove.Add(type);
                continue;
            }

            effect.PlayerUpdate(this);
        }

        foreach (var name in toRemove) {
            effects.Remove(name);
        }
    }



    private void UpdateHit() {
        if (!isHit) return;

        if (Game.Time >= hitTime + hitDuration) {

            isHit = false;

            HeartCount--;

            if (HeartCount == 1) Game.Sounds.Play("low_hearts");

            if (HeartCount <= 0) {
                Die();
                return;
            }

            ApplyKnockback(AngleToOpponent + MathF.PI, 100f, Time.InSeconds(1f));
        }
    }



    private void UpdateKnockback() {
        if (!isKnockbacked) return;

        if (Game.Time - knockbackTime >= knockbackDuration) {
            isKnockbacked = false;

            CanMove = true;
            CanAttack = true;
            CanBomb = true;
            return;
        }

        var t = MathF.Min((Game.Time - knockbackTime) / (float)knockbackDuration, 1f);

        Position = (knockbackEndPosition - knockbackStartPosition) * Easing.Out(t, 5f) + knockbackStartPosition;
    }



    private void UpdateMovement() {
        var movementVector = CanMove ? new Vector2(
            (Game.IsActionPressed(PlayerActions.Right) ? 1f : 0f) - (Game.IsActionPressed(PlayerActions.Left) ? 1f : 0f),
            (Game.IsActionPressed(PlayerActions.Up) ? 1f : 0f) - (Game.IsActionPressed(PlayerActions.Down) ? 1f : 0f)
        ) : Vector2.Zero;

        float movementAngle = MathF.Atan2(movementVector.Y, movementVector.X);

        movementAngleVector = new Vector2(
            MathF.Cos(movementAngle),
            MathF.Sin(movementAngle)
        );


        var modifier = Game.Time - movespeedModifierTime >= movespeedModifierDuration ? 1f : movespeedModifier;

        var velocityVector = CanMove ? (new Vector2(
            MathF.Abs(movementAngleVector.X) * movementVector.X,
            MathF.Abs(movementAngleVector.Y) * movementVector.Y)
            * (Focused ? FocusedSpeed : Speed) * modifier)
            : Vector2.Zero;


        ChangeVelocity(velocityVector);

        Position += Velocity * Game.Delta.AsSeconds();
    }

    private void InvokeActionPresses(PlayerActions action) {

        // attacks
        if (CanAttack && attacks.TryGetValue(action, out var attack)) {

            if (attack.Cooldown <= 0) attack.Cooldown = 0;
            else attack.Cooldown -= Game.Delta;

            if (Power < attack.Cost || attack.Cooldown > 0 || attack.Disabled) return;

            Time cooldownOverflow = Math.Abs(attack.Cooldown);

            // when attack is buffered
            if (Game.Input.IsActionPressBuffered(action, out _, out var state)) {
                Game.Input.ConsumePressBuffer(action);

                bool focused = state.HasFlag(PlayerActions.Focus);

                attack.PlayerPress(this, cooldownOverflow, focused);
                if (attack.Holdable) currentlyHeldAttacks.Add(attack, (Game.Time - cooldownOverflow, focused));

                //Log.Info($"buffer: {action}, {attack.Cooldown}");
            }

            // when attack is held but not necessarily buffered
            else if (attack.Holdable && Game.IsActionPressed(action) && !currentlyHeldAttacks.ContainsKey(attack)) {
                bool focused = Game.IsActionPressed(PlayerActions.Focus);

                attack.PlayerPress(this, cooldownOverflow, focused);
                currentlyHeldAttacks.Add(attack, (Game.Time - cooldownOverflow, focused));

                //Log.Info($"Attack held: {action}, {attack.Cooldown}");

            }
        }

        // bomb
        if (CanBomb && BombCount > 0 && Bomb.Action == action) {



            var bomb = Bomb.Bomb;

            if (bomb.Cooldown <= 0) bomb.Cooldown = 0;
            else bomb.Cooldown -= Game.Delta;

            if (bomb.Cooldown > 0) return;

            if (Game.Input.IsActionPressBuffered(action, out _, out var state)) {
                Game.Input.ConsumePressBuffer(action);

                Time cooldownOverflow = Math.Abs(bomb.Cooldown);
                bool focused = state.HasFlag(PlayerActions.Focus);

                bomb.PlayerPress(this, cooldownOverflow, focused);

                // death bomb
                if (isHit) {
                    isHit = false;

                    CanMove = true;
                    CanAttack = true;
                }

                BombCount--;
            }
        }
    }

    private void InvokeActionHolds(PlayerActions action) {
        if (attacks.TryGetValue(action, out var attack)) {
            if (currentlyHeldAttacks.TryGetValue(attack, out var heldState)) {



                if (attack.Cooldown <= 0) attack.PlayerHold(this, Math.Abs(attack.Cooldown), Game.Time - heldState.Time, heldState.Focused);

                if (Game.Input.IsActionReleaseBuffered(action) || Power < attack.Cost) {
                    attack.PlayerRelease(this, Math.Abs(attack.Cooldown), Game.Time - heldState.Time, heldState.Focused);

                    currentlyHeldAttacks.Remove(attack);
                }
            }
        }
    }







    public override void Render() {

        if (Focused) {

            hitboxSprite.Position = Position;
            hitboxSprite.Color = Color;
            hitboxSprite.Depth = 1;

            Game.Draw(hitboxSprite, Layer.Foreground1);
        }

        foreach (var attack in attacks.Values) {
            attack.PlayerRender(this);
        }
    }

    public void Hit(Entity entity, Hitbox hitbox) {


        if (entity is Projectile projectile) {

            if (Game.Time < InvulnerabilityTime + InvulnerabilityDuration || isDead) return;

            if (hitbox.CollisionGroup == CollisionGroup.OpponentProjectileMinor) projectile.NetworkDestroy();

            ApplyInvulnerability(Time.InSeconds(2.5f));
            ApplyHit(Time.InMilliseconds(133.333)); // 8 frames
            //ApplyHit(Time.InMilliseconds(2000)); // 2s

        }
    }

    private void ApplyHit(Time duration) {

        isHit = true;

        CanMove = false;
        CanAttack = false;

        hitTime = Game.Time;
        hitDuration = duration;

        Scene.AddEntity(new HitExplosion(Position, 0.5f, 100f, Color));

        var hitPacket = new Packet(PacketType.Hit).In(Position);
        Game.Network.Send(hitPacket);

        Game.Sounds.Play("hit");
    }

    private void Die() {

        isDead = true;
        deathTime = Game.Network.Time;

        Scene.AddEntity(new HitExplosion(Position, 1f, 500f, Color));

        Game.Sounds.Play("death");

        var packet = new Packet(PacketType.Death).In(deathTime).In(Position);
        Game.Network.Send(packet);
    }

    public void Graze(Entity entity, Hitbox hitbox) {
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
        isKnockbacked = true;

        CanMove = false;
        CanAttack = false;
        CanBomb = false;

        Velocity = new Vector2(0f, 0f);
        knockbackTime = Game.Time;
        knockbackStartPosition = Position;
        knockbackEndPosition = Position + new Vector2(strength * MathF.Cos(angle), strength * MathF.Sin(angle));
        knockbackDuration = duration;

        var packet = new Packet(PacketType.Knockback).In(Game.Network.Time).In(Position).In(angle);
        Game.Network.Send(packet);
    }

    public void ApplyInvulnerability(Time duration) {
        InvulnerabilityTime = Game.Time;
        InvulnerabilityDuration = duration;
    }

    public void ApplyAttackCooldowns(Time duration, params PlayerActions[] actions) {
        foreach (var action in actions) {
            if (attacks.TryGetValue(action, out var attack) && duration > attack.Cooldown) {
                attack.CooldownDuration = duration;
                attack.Cooldown = duration;
            }
        }

    }

    public void ApplyEffect<T>(T effect) where T : Effect {
        if (!effects.ContainsKey(typeof(T))) {
            effects.TryAdd(typeof(T), effect);
        }
    }

    public bool GetEffect<T>(out T effect) where T : Effect {
        var type = typeof(T);
        if (effects.ContainsKey(type)) {
            effect = (T)effects[type];
            return true;
        }
        effect = null;
        return false;
    }

    public bool HasEffect<T>() where T : Effect {
        var type = typeof(T);
        return (
            effects.ContainsKey(type) &&
            !effects[type].HasTimedOut &&
            !effects[type].IsCanceled);
    }

    public void CancelEffect<T>() where T : Effect {
        if (effects.TryGetValue(typeof(T), out var effect)) {
            effect.Cancel();
        }
    }



    public void DisableAttacks(params PlayerActions[] actions) {
        foreach (var action in actions) {
            if (attacks.TryGetValue(action, out var attack)) attack.Disable();
        }
    }

    public void EnableAttacks(params PlayerActions[] actions) {
        foreach (var action in actions) {
            if (attacks.TryGetValue(action, out var attack)) attack.Enable();
        }
    }

    public T GetAttack<T>(PlayerActions action) where T : Attack {
        return attacks[action] as T;
    }



    public void SpendPower(int amount) {
        var powerOverflow = Math.Max((Match.TotalPowerGenerated + powerGainedFromGrazing - powerSpent) - 400, 0);

        int spent = powerOverflow + amount;
        powerSpent += spent;

        var packet = new Packet(PacketType.SpentPower).In(spent);
        Game.Network.Send(packet);
    }



    private void ChangeVelocity(Vector2 newVelocity) {
        if (newVelocity == Velocity) return;
        Velocity = newVelocity;
        var packet = new Packet(PacketType.VelocityChanged);
        packet.In((long)Game.Network.Time).In(Position).In(Velocity).In(Focused);
        Game.Network.Send(packet);
    }



    public void Receive(Packet packet, IPEndPoint endPoint) {
        if (packet.Type == PacketType.Death) {
            ApplyInvulnerability(Time.InSeconds(999f));
        }
    }



    protected void AddAttack(PlayerActions action, Attack attack) => attacks[action] = attack;
    protected void AddBomb(PlayerActions action, Bomb bomb) => Bomb = (action, bomb);

    public void ApplyMovespeedModifier(float modifier) {
        ApplyMovespeedModifier(modifier, long.MaxValue);
    }

    public void ApplyMovespeedModifier(float modifier, Time duration) {
        movespeedModifier = modifier;
        movespeedModifierDuration = duration;
        movespeedModifierTime = Game.Time;
    }
}