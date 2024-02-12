using System.Collections.ObjectModel;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Characters;




public class Character : Entity {



    public Color4 Color { get; init; }

    public float Speed { get; protected init; }
    public float FocusedSpeed { get; protected init; }


    public bool IsP1 { get; }
    public bool IsPlayer { get; }


    public CharacterState State { get; protected set; }



    public bool IsFocused { get; protected set; }
    public Vector2 Velocity { get; protected set; }
    public Vector2 MovementVector { get; protected set; }
    public float MovespeedModifier { get; protected set; }
    public Timer MovespeedModifierTimer { get; protected set; }


    public int Power { get => Math.Min((int)(Match.TotalPowerGenerated + PowerGainedFromGrazing - PowerSpent), 400); }
    public int PowerGainedFromGrazing { get; protected set; }
    public int PowerSpent { get; protected set; }


    public int BombCount { get; protected set; } = 3;




    public int HeartCount { get; protected set; } = 5;

    public bool IsInvulnerable { get => !InvulnerabilityTimer.HasFinished; }

    public Timer InvulnerabilityTimer { get; protected set; }

    public Timer HitTimer { get; protected set; }
    public Timer KnockbackTimer { get; protected set; }

    public Time DeathTime { get; protected set; }


    public Character Opponent { get => opponent ??= Scene.GetFirstEntityWhere<Character>(e => e != this); }
    private Character opponent;

    public float AngleToOpponent {
        get {
            var d = Opponent.Position - Position;
            return MathF.Atan2(d.Y, d.X);
        }
    }

    public float DistanceToOpponent {
        get {
            var d = Opponent.Position - Position;
            return MathF.Sqrt(d.X * d.X + d.Y * d.Y);
        }
    }





    public Match Match { get => match ??= Scene.GetFirstEntity<Match>(); }
    private Match match;

    private Vector2 basePosition;
    private Vector2 interpolationOffset;
    private Timer interpolationTimer;

    private List<(Vector2 Offset, Timer Timer, Func<float, float> Easing)> interpolations = new();

    private Dictionary<PlayerActions, Attack> attacks = new();
    private Dictionary<PlayerActions, (Time CooldownOverflow, Time HoldTime, bool IsFocused)> heldAttacks = new();

    private Bomb bomb;

    private Vector2 knockbackStartPosition;
    private Vector2 knockbackEndPosition;


    public Character(bool isP1, bool isPlayer, Color4 color) {
        IsP1 = isP1;
        IsPlayer = isPlayer;


        Color = color;

        basePosition = new Vector2(isP1 ? -200f : 200f, 0f);

    }



    public override void Update() {
        switch (State) {
            case CharacterState.Free:
                UpdateMovement();
                break;

            case CharacterState.Knockbacked:
                UpdateKnockback();
                break;
        }

        basePosition = new Vector2(
            Math.Clamp(basePosition.X, -Match.Bounds.X, Match.Bounds.X),
            Math.Clamp(basePosition.Y, -Match.Bounds.Y, Match.Bounds.Y)
        );

        Position = basePosition;

        foreach (var interpolation in interpolations) {
            Position += interpolation.Offset * interpolation.Easing(interpolation.Timer.RemainingRatio);
        }

        Position = new Vector2(
            Math.Clamp(Position.X, -Match.Bounds.X, Match.Bounds.X),
            Math.Clamp(Position.Y, -Match.Bounds.Y, Match.Bounds.Y)
        );



    }

    public override void Render() {
        foreach (var attack in attacks) {
            attack.Value.Render();
        }

        RenderArrow();
        RenderMiniHud();
    }



    private void RenderArrow() {
        var arrow = new Sprite("opponentarrow") {
            Origin = new Vector2(-2.5f, 0.5f),
            Position = Position,
            Scale = new Vector2(0.2f),
            Rotation = AngleToOpponent,
            Color = new Color4(Color.R, Color.G, Color.B, 0.5f),
            UseColorSwapping = true,
        };

        Game.Draw(arrow, IsPlayer ? Layer.Player : Layer.Opponent);
    }

    private void RenderMiniHud() {

        var barWidth = 50f;

        var powerBarBG = new Rectangle {
            Size = new Vector2(barWidth, 6f),
            FillColor = new Color4(1f, 1f, 1f, 0.1f),
            StrokeColor = new Color4(1f, 1f, 1f, 0.2f),
            StrokeWidth = 1f,
            Origin = new Vector2(0f, 0.5f),
            Position = Position + new Vector2(barWidth / -2f, -70f),
        };

        var powerBar = new Rectangle(powerBarBG) {
            Size = new Vector2(Power / 400f * barWidth, 5f),
            FillColor = new Color4(1f, 1f, 1f, 0.3f),
            StrokeWidth = 0f,
        };

        Game.Draw(powerBarBG, IsPlayer ? Layer.Player : Layer.Opponent);
        Game.Draw(powerBar, IsPlayer ? Layer.Player : Layer.Opponent);

        float spacing = 8f;

        for (int i = 0; i < BombCount; i++) {
            var circle = new Circle {
                Radius = 2f,
                FillColor = new Color4(1f, 1f, 1f, 0.5f),
                Origin = new Vector2(0.5f),
                Position = Position + new Vector2(spacing * i - (BombCount - 1) * spacing / 2f, -78f),
            };

            Game.Draw(circle, IsPlayer ? Layer.Player : Layer.Opponent);
        }

    }

    public void Focus() => IsFocused = true;
    public void Unfocus() => IsFocused = false;

    public void SetPosition(Vector2 position) {
        State = CharacterState.Free;

        basePosition = position;
    }

    public void AddInterpolation(Vector2 offset, Time duration, Func<float, float> function) {
        interpolations.Add((offset, new Timer(duration), function));
    }

    public void ResetInterpolations() => interpolations.Clear();

    public void InterpolatePosition(Vector2 offset, Time duration) {
        interpolationOffset = offset;
        interpolationTimer = new Timer(duration);
    }

    public void SetVelocity(Vector2 velocity) {
        State = CharacterState.Free;
        Velocity = velocity;

    }

    public void ApplyMovespeedModifier(float modifier) {
        MovespeedModifier = modifier;
        MovespeedModifierTimer = Timer.MaxDuration();
    }

    public void ApplyMovespeedModifier(float modifier, Time duration) {
        MovespeedModifier = modifier;
        MovespeedModifierTimer = new Timer(duration);
    }



    public bool IsAttackAvailable(PlayerActions action) {
        var attack = attacks[action];

        return (Power >= attack.Cost && attack.CooldownTimer.HasFinished && !attack.IsDisabled);

    }

    public bool IsAttackHoldable(PlayerActions action) => attacks[action].IsHoldable;

    public bool IsAttackHeld(PlayerActions action) => heldAttacks.ContainsKey(action);

    public bool GetAttackHoldState(PlayerActions action, out (Time CooldownOverflow, Time HeldTime, bool IsFocused) state) {
        if (IsAttackHeld(action)) {
            state = heldAttacks[action];
            return true;
        }
        state = default;
        return false;
    }

    public Timer GetAttackCooldownTimer(PlayerActions action) => attacks[action].CooldownTimer;
    public int GetAttackCost(PlayerActions action) => attacks[action].Cost;
    public bool IsAttackFocusable(PlayerActions action) => attacks[action].HasFocusVariant;

    public string GetAttackIconName(PlayerActions action, bool isFocused) {
        var attack = attacks[action];
        return isFocused && attack.HasFocusVariant ? attack.FocusedIcon : attack.Icon;
    }

    public void PressLocalAttack(PlayerActions action, Time cooldownOverflow, bool isFocused) {

        var attack = attacks[action];

        attack.LocalPress(cooldownOverflow, isFocused);

        State = CharacterState.Free;


    }

    public void PressLocalAttack(PlayerActions action, Time cooldownOverflow, Time holdTime, bool isFocused) {

        var attack = attacks[action];

        attack.LocalPress(cooldownOverflow, isFocused);
        if (attack.IsHoldable) heldAttacks.Add(action, (cooldownOverflow, holdTime, isFocused));

        State = CharacterState.Free;

    }

    public void HoldLocalAttack(PlayerActions action) {

        if (!heldAttacks.TryGetValue(action, out var holdState)) return;

        var attack = attacks[action];

        attack.LocalHold(holdState.CooldownOverflow, Game.Time - holdState.HoldTime, holdState.IsFocused);

    }

    public void ReleaseLocalAttack(PlayerActions action) {

        if (!heldAttacks.TryGetValue(action, out var holdState)) return;

        var attack = attacks[action];

        attack.LocalRelease(holdState.CooldownOverflow, Game.Time - holdState.HoldTime, holdState.IsFocused);
        heldAttacks.Remove(action);

    }

    public void ReleaseRemoteAttack(PlayerActions action, Packet packet) {
        attacks[action].RemoteRelease(packet);

    }



    public bool IsBombAvailable() {
        return (BombCount > 0 && bomb.CooldownTimer.HasFinished);
    }

    public Timer GetBombCooldownTimer() => bomb.CooldownTimer;

    public void PressLocalBomb(Time cooldownOverflow, bool isFocused) {

        bomb.LocalPress(cooldownOverflow, isFocused);
        BombCount--;

        State = CharacterState.Free;
    }

    internal void PressRemoteBomb(Packet packet) {
        bomb.RemotePress(packet);
        BombCount--;

        State = CharacterState.Free;

    }



    public void ApplyAttackCooldowns(Time duration, params PlayerActions[] actions) {
        foreach (var action in actions) attacks[action].ApplyCooldown(duration);

    }

    public void DisableAttacks(params PlayerActions[] actions) {
        foreach (var action in actions) attacks[action].Disable();
    }

    public void EnableAttacks(params PlayerActions[] actions) {
        foreach (var action in actions) attacks[action].Enable();
    }



    public int SpendPower(int amount, bool overflow = true) {

        int amountActuallySpent = amount;

        if (overflow) {
            var powerOverflow = Math.Max((int)((Match.TotalPowerGenerated + PowerGainedFromGrazing - PowerSpent) - 400), 0);
            amountActuallySpent += powerOverflow;
        }

        PowerSpent += amountActuallySpent;

        return amountActuallySpent;
    }


    public void ApplyInvulnerability() => InvulnerabilityTimer = Timer.MaxDuration();
    public void ApplyInvulnerability(Time duration) => InvulnerabilityTimer = new Timer(duration);


    public void Graze(int grazeAmount) => PowerGainedFromGrazing += grazeAmount;


    public void Hit() {
        State = CharacterState.Hit;
        HitTimer = Timer.MaxDuration();
    }
    public void Hit(Time duration) {
        State = CharacterState.Hit;
        HitTimer = new Timer(duration);
    }

    public void Damage() => HeartCount = Math.Max(HeartCount - 1, 0);


    public void Knockback(float direction, float strength, Time duration) {
        State = CharacterState.Knockbacked;
        knockbackStartPosition = Position;
        knockbackEndPosition = Position + new Vector2(MathF.Cos(direction), MathF.Sin(direction)) * strength;
        KnockbackTimer = new Timer(duration);
    }




    public void Die(Time time) {

        State = CharacterState.Dead;
        DeathTime = time;
    }

    public virtual Entity GetController(ControllerType type) {
        return (type) switch {
            ControllerType.LocalNetplay => new LocalCharacterController<Character>(this),
            ControllerType.RemoteNetplay => new RemoteCharacterController<Character>(this),
            _ => throw new Exception($"Character controller does not exist for controller type {type}")
        };
    }





    protected void InitMoveset(
        Attack primary,
        Attack secondary,
        Attack special,
        Attack super,
        Bomb bomb) {

        attacks[PlayerActions.Primary] = primary;
        attacks[PlayerActions.Secondary] = secondary;
        attacks[PlayerActions.Special] = special;
        attacks[PlayerActions.Super] = super;

        this.bomb = bomb;
    }



    private void UpdateMovement() => basePosition += Velocity * Game.Delta.AsSeconds();

    private void UpdateKnockback() {
        var t = 1f - KnockbackTimer.Remaining.AsSeconds() / KnockbackTimer.Duration.AsSeconds();
        basePosition = knockbackStartPosition + (knockbackEndPosition - knockbackStartPosition) * Easing.Out(t, 5f);
    }


}