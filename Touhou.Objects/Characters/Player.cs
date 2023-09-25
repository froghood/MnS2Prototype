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


    public bool Focused { get => Game.IsActionPressed(PlayerActions.Focus); }

    public bool CanMove { get; private set; } = true;
    public bool CanAttack { get; private set; } = true;
    public Time InvulnerabilityTime { get; private set; }
    public Time InvulnerabilityDuration { get; private set; }

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

    //private Shader cooldownShader;

    private Dictionary<Attack, (Time Time, bool Focused)> currentlyHeldAttacks = new();



    public Opponent Opponent => opponent is null ? opponent = Scene.GetFirstEntity<Opponent>() : opponent;
    private Opponent opponent;


    private bool hosting;
    private bool isDeathConfirmed;
    private Time deathConfirmationTime;



    private Sprite characterSprite;
    private Sprite hitboxSprite;



    private bool isHit;
    private Time hitTime;
    private Time hitDuration;


    public Color4 Color { get; set; } = new Color4(0.4f, 1f, 0f, 1f);


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
        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), 0.5f, CollisionGroups.Player, Hit));
        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), 50f, CollisionGroups.Player, Graze));

        //cooldownShader = new Shader(null, null, "assets/shaders/cooldown.frag");
        characterSprite = new Sprite("reimu") {
            Origin = new Vector2(0.4f, 0.3f),
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
                Game.Scenes.PopScene();
                Game.Scenes.PushScene<MatchScene>(hosting, matchStartTime);
            });
        }

        if (!Match.Started || isDead) return;

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

        var velocityVector = CanMove ? (new Vector2(
            MathF.Abs(MathF.Cos(movementAngle)) * movementVector.X,
            MathF.Abs(MathF.Sin(movementAngle)) * movementVector.Y)
            * (Focused ? FocusedSpeed : Speed) * MovespeedModifier)
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

                //System.Console.WriteLine($"buffer: {action}, {attack.Cooldown}");
            }

            // when attack is held but not necessarily buffered
            else if (attack.Holdable && Game.IsActionPressed(action) && !currentlyHeldAttacks.ContainsKey(attack)) {
                bool focused = Game.IsActionPressed(PlayerActions.Focus);

                attack.PlayerPress(this, cooldownOverflow, focused);
                currentlyHeldAttacks.Add(attack, (Game.Time - cooldownOverflow, focused));

                System.Console.WriteLine($"held: {action}, {attack.Cooldown}");

            }
        }

        // bomb
        if (CanBomb && BombCount > 0 && Bomb.Action == action) {



            var bomb = Bomb.Bomb;

            if (bomb.Cooldown <= 0) bomb.Cooldown = 0;
            else bomb.Cooldown -= Game.Delta;

            if (bomb.Cooldown > 0) return;

            //System.Console.WriteLine("t");

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

        if (!isDead) {

            characterSprite.Position = Position;
            characterSprite.Scale = new Vector2(MathF.Sign(Position.X - Opponent.Position.X), 1f) * 0.2f;

            Game.Draw(characterSprite, Layers.Player);



            if (Focused) {

                hitboxSprite.Position = Position;
                hitboxSprite.Color = Color;
                hitboxSprite.Depth = 1;

                Game.Draw(hitboxSprite, Layers.Foreground1);
            }


        }

        foreach (var attack in attacks.Values) {
            attack.PlayerRender(this);
        }
    }

    public void Hit(Entity entity) {


        if (entity is Projectile projectile) {

            if (Game.Time < InvulnerabilityTime + InvulnerabilityDuration || isDead) return;

            projectile.Destroy();

            // must toggle the last bit because the opponents' projectile ids are opposite
            var destroyProjectilePacket = new Packet(PacketType.DestroyProjectile).In(projectile.Id ^ 0x80000000);
            Game.Network.Send(destroyProjectilePacket);

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

    private void ApplyInvulnerability(Time duration) {
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

                System.Console.WriteLine("t");

                packet.Out(out Time startTime, true);

                Game.Command(() => {
                    Game.Scenes.PopScene();
                    Game.Scenes.PushScene<MatchScene>(hosting, startTime);
                });
                break;

        }
    }

    protected void AddAttack(PlayerActions action, Attack attack) => attacks[action] = attack;
    protected void AddBomb(PlayerActions action, Bomb bomb) => Bomb = (action, bomb);

}