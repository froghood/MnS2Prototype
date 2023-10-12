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


    public bool IsDead { get => isDead; }
    public Time DeathTime { get => deathTime; }

    private bool isKnockbacked;
    private Timer knockbackTimer;
    private Vector2 knockbackStartPosition;
    private Vector2 knockbackEndPosition;




    public IEnumerable<KeyValuePair<PlayerActions, Attack>> Attacks { get => attacks.AsEnumerable(); }


    // power
    public int Power { get => Math.Min(Match.TotalPowerGenerated + powerGainedFromGrazing - powerSpent, 400); }
    private int powerGainedFromGrazing;
    private int powerSpent;
    public float SmoothPower { get => smoothPower; }
    private float smoothPower;

    // hearts
    public int HeartCount { get; private set; } = 5;
    private bool isDead;
    private Time deathTime;

    public bool CanBomb { get; private set; } = true;
    public int BombCount { get; set; } = 3;

    public Match Match => match is null ? match = Scene.GetFirstEntity<Match>() : match;
    private Match match;


    private Dictionary<PlayerActions, Attack> attacks = new();
    private Bomb bomb;


    private Dictionary<Attack, (Time CooldownOverflow, Time HeldTime, bool Focused)> currentlyHeldAttacks = new();



    protected Opponent Opponent => opponent ??= Scene.GetFirstEntity<Opponent>();
    private Opponent opponent;


    private bool isP1;
    private bool isDeathConfirmed;
    private Time deathConfirmationTime;



    private Sprite characterSprite;
    private Sprite hitboxSprite;



    private bool isHit;
    private Timer hitTimer;
    private Dictionary<Type, Effect> effects = new();
    private Vector2 movementAngleVector;


    private Timer movespeedModifierTimer;
    private float movespeedModifier;


    private Timer invulnerabilityTimer;

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
        foreach (var action in order) ProcessActionPresses(action);
        foreach (var action in order) ProcessActionHolds(action);
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

        if (hitTimer.HasFinished) {

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

        if (knockbackTimer.HasFinished) {
            isKnockbacked = false;

            CanMove = true;
            CanAttack = true;
            CanBomb = true;

        } else {

            var t = 1f - knockbackTimer.Remaining.AsSeconds() / knockbackTimer.Duration.AsSeconds();
            Position = knockbackStartPosition + (knockbackEndPosition - knockbackStartPosition) * Easing.Out(t, 5f);

        }

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


        var modifier = movespeedModifierTimer.HasFinished ? 1f : movespeedModifier;

        var velocityVector = CanMove ? (new Vector2(
            MathF.Abs(movementAngleVector.X) * movementVector.X,
            MathF.Abs(movementAngleVector.Y) * movementVector.Y)
            * (Focused ? FocusedSpeed : Speed) * modifier)
            : Vector2.Zero;


        ChangeVelocity(velocityVector);

        Position += Velocity * Game.Delta.AsSeconds();
    }

    private void ProcessActionPresses(PlayerActions action) {

        // attacks
        if (CanAttack && attacks.TryGetValue(action, out var attack) && Power >= attack.Cost && attack.CooldownTimer.HasFinished && !attack.Disabled) {

            if (attack.Holdable && !currentlyHeldAttacks.ContainsKey(attack)) { // holdable
                ProcessHoldable(attack);
            }

            if (!attack.Holdable) { // not holdable
                ProcessNonHoldable(attack);
            }
        }

        // bomb
        if (CanBomb && BombCount > 0 && action == PlayerActions.Bomb && bomb.CooldownTimer.HasFinished) {
            ProcessBomb();
        }



        void ProcessHoldable(Attack attack) {

            if (Game.IsActionPressed(action)) {

                bool focused = Game.IsActionPressed(PlayerActions.Focus);

                var pressTime = Game.Input.GetLastPressTime(action);

                var heldTime = pressTime < attack.CooldownTimer.FinishTime ? attack.CooldownTimer.FinishTime : Game.Time;

                Log.Info($"Pressed action {action} | cooldown overflow: {0L}μs | held time offset: {Game.Time - heldTime}");

                attack.PlayerPress(this, 0L, focused);
                currentlyHeldAttacks.TryAdd(attack, (0L, heldTime, focused));

            } else {

                var releaseTime = Game.Input.GetLastReleaseTime(action);

                // niche case for when a release happens on the next available frame after the attack's cooldown ends

                // must check if the cooldown finish time happened inbetween the current and previous frame times;
                // required in order to stop unintended activations before the attack's cooldown timer has been set for the first time
                if (releaseTime >= attack.CooldownTimer.FinishTime && (Game.Time - Game.Delta) <= attack.CooldownTimer.FinishTime) {

                    bool focused = Game.IsActionPressed(PlayerActions.Focus);

                    var heldTime = attack.CooldownTimer.FinishTime;

                    Log.Warn($"Released action {action} | cooldown overflow: {0L}μs | held time offset: {Game.Time - heldTime}");

                    attack.PlayerPress(this, 0L, focused);
                    currentlyHeldAttacks.TryAdd(attack, (0L, heldTime, focused));

                } else {

                    if (!Game.Input.IsActionPressBuffered(action, out var bufferPressTime, out var bufferState)) return;

                    Game.Input.ConsumePressBuffer(action);

                    var cooldownOverflow = Game.Time - attack.CooldownTimer.FinishTime;

                    Log.Info($"Buffered action {action} | cooldown overflow: {cooldownOverflow}μs");

                    attack.PlayerPress(this, cooldownOverflow, bufferState.HasFlag(PlayerActions.Focus));
                    currentlyHeldAttacks.TryAdd(attack, (cooldownOverflow, Game.Time, bufferState.HasFlag(PlayerActions.Focus)));
                }
            }
        }

        void ProcessNonHoldable(Attack attack) {
            if (!Game.Input.IsActionPressBuffered(action, out var bufferPressTime, out var bufferState)) return;

            Game.Input.ConsumePressBuffer(action);

            var cooldownOverflow = (bufferPressTime < attack.CooldownTimer.FinishTime) ?
                Game.Time - attack.CooldownTimer.FinishTime : (Time)0L;

            attack.PlayerPress(this, cooldownOverflow, bufferState.HasFlag(PlayerActions.Focus));
        }

        void ProcessBomb() {
            if (!Game.Input.IsActionPressBuffered(action, out var bufferPressTime, out var bufferState)) return;

            Game.Input.ConsumePressBuffer(action);

            var cooldownOverflow = (bufferPressTime < bomb.CooldownTimer.FinishTime) ?
                Game.Time - bomb.CooldownTimer.FinishTime : (Time)0L;

            bomb.PlayerPress(this, cooldownOverflow, bufferState.HasFlag(PlayerActions.Focus));

            // death bomb
            if (isHit) {
                isHit = false;

                CanMove = true;
                CanAttack = true;
            }

            BombCount--;
        }
    }

    private void ProcessActionHolds(PlayerActions action) {
        if (attacks.TryGetValue(action, out var attack)) {
            if (currentlyHeldAttacks.TryGetValue(attack, out var heldState)) {



                if (attack.CooldownTimer.HasFinished) attack.PlayerHold(this, heldState.CooldownOverflow, Game.Time - heldState.HeldTime, heldState.Focused);

                if (!Game.IsActionPressed(action) || Power < attack.Cost) {

                    attack.PlayerRelease(this, heldState.CooldownOverflow, Game.Time - heldState.HeldTime, heldState.Focused);
                    currentlyHeldAttacks.Remove(attack);
                }
            }
        }
    }







    public override void Render() {

        var opponentArrow = new Sprite("opponentarrow") {
            Origin = new Vector2(-2f, 0.5f),
            Position = Position,
            Scale = new Vector2(0.2f),
            Rotation = AngleToOpponent,
            Color = new Color4(Color.R, Color.G, Color.B, 0.5f),
            UseColorSwapping = true,
        };



        var hitbox = new Sprite(Focused ? "hitboxfocused" : "hitboxunfocused") {
            Origin = new Vector2(0.5f),
            Position = Position,
            Scale = new Vector2(Focused ? 0.15f : 0.18f),
            Color = new Color4(Color.R, Color.G, Color.B, Focused ? 1f : 0.8f),
            UseColorSwapping = true,
        };



        float barWidth = 50f;

        var powerBarBG = new Rectangle {
            Size = new Vector2(barWidth, 6f),
            FillColor = new Color4(1f, 1f, 1f, 0.1f),
            StrokeColor = new Color4(1f, 1f, 1f, 0.2f),
            StrokeWidth = 1f,
            Origin = new Vector2(0f, 0.5f),
            Position = Position + new Vector2(barWidth / -2f, -30f),
        };

        var powerBar = new Rectangle(powerBarBG) {
            Size = new Vector2(Power / 400f * barWidth, 5f),
            FillColor = new Color4(1f, 1f, 1f, 0.3f),
            StrokeWidth = 0f,
        };

        Game.Draw(powerBarBG, Layer.Player);
        Game.Draw(powerBar, Layer.Player);



        Game.Draw(opponentArrow, Layer.Player);
        Game.Draw(hitbox, Layer.Player);



        foreach (var attack in attacks.Values) {
            attack.PlayerRender(this);
        }
    }

    public void Hit(Entity entity, Hitbox hitbox) {


        if (entity is Projectile projectile) {

            if (!invulnerabilityTimer.HasFinished || isDead) return;

            if (hitbox.CollisionGroup == CollisionGroup.OpponentProjectileMinor) projectile.NetworkDestroy();

            ApplyInvulnerability(Time.InSeconds(2.5f));
            ApplyHit(Time.InMilliseconds(133.333)); // 8 frames
            //ApplyHit(Time.InMilliseconds(2000)); // 2s

        }
    }

    private void ApplyHit(Time duration) {

        isHit = true;
        hitTimer = new Timer(duration);

        CanMove = false;
        CanAttack = false;

        Scene.AddEntity(new HitExplosion(Position, 0.5f, 100f, Color));

        Game.Sounds.Play("hit");

        var hitPacket = new Packet(PacketType.Hit).In(Position);
        Game.Network.Send(hitPacket);


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
        knockbackTimer = new Timer(duration);
        knockbackStartPosition = Position;
        knockbackEndPosition = Position + new Vector2(strength * MathF.Cos(angle), strength * MathF.Sin(angle));

        CanMove = false;
        CanAttack = false;
        CanBomb = false;

        Velocity = new Vector2(0f, 0f);

        var packet = new Packet(PacketType.Knockback).In(Game.Network.Time).In(Position).In(angle);
        Game.Network.Send(packet);
    }

    public void ApplyInvulnerability(Time duration) {
        invulnerabilityTimer = new Timer(duration);
    }

    public void ApplyAttackCooldowns(Time duration, params PlayerActions[] actions) {
        foreach (var action in actions) {
            if (attacks.TryGetValue(action, out var attack) && duration > attack.CooldownTimer.Remaining) {


                Log.Info($"Applying attack cooldown of {duration.AsSeconds()}s to {action}");

                attack.CooldownTimer = new Timer(duration);

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
    protected void AddBomb(Bomb bomb) => this.bomb = bomb;

    public void ApplyMovespeedModifier(float modifier) {
        ApplyMovespeedModifier(modifier, long.MaxValue);
    }

    public void ApplyMovespeedModifier(float modifier, Time duration) {
        movespeedModifier = modifier;
        movespeedModifierTimer = new Timer(duration);
    }
}