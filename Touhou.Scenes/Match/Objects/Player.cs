using System.Net;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Touhou.Net;
using Touhou.Objects;

using Touhou.Scenes.Match.Objects.Characters;

namespace Touhou.Scenes.Match.Objects;

public abstract class Player : Entity, IControllable, IReceivable {

    public float Speed { get; protected init; }
    public float FocusedSpeed { get; protected init; }

    public Vector2f Velocity { get; private set; }


    public bool Focused { get => Game.IsActionPressed(PlayerAction.Focus); }

    public bool CanMove { get; private set; } = true;

    public int HitCount { get; private set; }

    public Time InvulnerabilityTime { get; private set; }
    public Time InvulnerabilityDuration { get; private set; }
    public Time KnockbackTime { get; private set; }
    public Vector2f KnockbackStartPosition { get; private set; }
    public Vector2f KnockbackEndPosition { get; private set; }
    public Time KnockbackDuration { get; private set; }

    // power
    public int Power { get => Math.Min(Timer.TotalPowerGeneration + powerGainedFromGrazing - powerSpent, 400); }
    private int powerGainedFromGrazing;
    private int powerSpent;

    private float smoothPower;

    public MatchTimer Timer {
        get {
            timer ??= Scene.GetFirstEntity<MatchTimer>();
            return timer;
        }
    }
    private MatchTimer timer;


    private Dictionary<PlayerAction, Attack> attacks = new();

    private Shader cooldownShader;

    private Dictionary<Attack, (Time Time, bool Focused)> currentlyHeldAttacks = new();


    private Dictionary<int, Projectile> projectiles = new();

    private int totalSpawnedProjectiles = 0;


    public Opponent Opponent {
        get {
            opponent ??= Scene.GetFirstEntity<Opponent>();
            return opponent;
        }
    }
    private Opponent opponent;

    public float AngleToOpponent {
        get {
            var positionDifference = Opponent.Position - Position;
            return MathF.Atan2(positionDifference.Y, positionDifference.X);
        }
    }


    public float MovespeedModifier { get; set; } = 1f;

    public Player() {
        CanCollide = true;
        CollisionType = CollisionType.Player;
        CollisionGroups.Add(0);
        Hitboxes.Add(new CircleHitbox(this, new Vector2f(0f, 0f), 0.5f, Hit));
        Hitboxes.Add(new CircleHitbox(this, new Vector2f(0f, 0f), 25f, Graze));

        cooldownShader = new Shader(null, null, "assets/Cooldown.frag");
    }

    public virtual void Press(PlayerAction action) { }
    public virtual void Release(PlayerAction action) { }

    public override void Update() {

        if (!Timer.MatchStarted) return;

        UpdateKnockback();
        if (CanMove) UpdateMovement();

        Position = new Vector2f(
            Math.Clamp(Position.X, 5f, Game.Window.Size.X - 5f),
            Math.Clamp(Position.Y, 5f, Game.Window.Size.Y - 5f)
        );

        var order = Game.Input.GetActionOrder();

        foreach (var action in order) InvokeAttackPresses(action);
        foreach (var action in order) InvokeAttackHolds(action);
    }

    private void InvokeAttackPresses(PlayerAction action) {
        if (attacks.TryGetValue(action, out var attack)) {

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
        var easing = 1f - MathF.Pow(1f - t, 5f);

        Position = (KnockbackEndPosition - KnockbackStartPosition) * easing + KnockbackStartPosition;

    }

    public override void Render() {
        RenderCooldowns();
        RenderPower();

        foreach (var attack in attacks.Values) {
            attack.PlayerRender(this);
        }
    }

    private void RenderPower() {

        var rect = new RectangleShape();
        rect.Size = new Vector2f(
            Power / 400f * 204f,
            5
        );
        rect.Origin = new Vector2f(0f, rect.Size.Y);
        rect.Position = new Vector2f(4f, Game.Window.Size.Y - 106f);
        rect.FillColor = Color.White;

        Game.Window.Draw(rect);


        //smoothPower += MathF.Min(MathF.Abs(Power - smoothPower) * 0.99f, 50f * Game.Delta.AsSeconds()) * MathF.Sign(Power - smoothPower);

        smoothPower += MathF.Min(MathF.Abs(Power - smoothPower), Game.Delta.AsSeconds() * 80f) * MathF.Sign(Power - smoothPower);


        rect.Size = new Vector2f(
            -(Power - smoothPower) / 400 * 204f,
            5
        );
        rect.Position = new Vector2f(
            4f + Power / 400f * 204f,
            Game.Window.Size.Y - 106f
        );
        rect.FillColor = new Color(255, 200, 120);
        Game.Window.Draw(rect);


        var text = new Text();
        text.Font = Game.DefaultFont;
        text.CharacterSize = 14;
        text.DisplayedString = Power.ToString();
        text.Origin = new Vector2f(0f, text.GetLocalBounds().Height);
        text.Position = new Vector2f(5f, Game.Window.Size.Y - 40f);

        Game.Window.Draw(text);

        text.DisplayedString = smoothPower.ToString();
        text.Origin = new Vector2f(0f, text.GetLocalBounds().Height);
        text.Position = new Vector2f(5f, Game.Window.Size.Y - 26f);

        Game.Window.Draw(text);
    }

    public override void PostRender() { }

    public void Hit(Entity entity) {
        if (Game.Time - InvulnerabilityTime < InvulnerabilityDuration) return;

        System.Console.WriteLine("collide");

        if (entity is Projectile projectile) {

            System.Console.WriteLine("projectile");

            var angleToOpponent = AngleToOpponent + MathF.PI;

            HitCount++;

            ApplyInvulnerability(Time.InSeconds(2)); // 2s
            ApplyKnockback(angleToOpponent, 100f, Time.InSeconds(1)); // 1s

            //projectile.Destroy();

            var packet = new Packet(PacketType.Hit).In(Game.Network.Time).In(Position).In(angleToOpponent);
            Game.Network.Send(packet);

        }
    }

    public void Graze(Entity entity) {
        if (entity is Projectile projectile) {
            if (projectile.Grazed) return;
            powerGainedFromGrazing += projectile.GrazeAmount;
            projectile.Graze();
        }
    }

    private void ApplyKnockback(float angle, float strength, Time duration) {
        CanMove = false;
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
            if (attacks.TryGetValue(action, out var attack)) {
                attack.CooldownDuration = duration;
                attack.Cooldown = duration;
            }
        }

    }

    public void DisableAttacks(params PlayerAction[] actions) {
        foreach (var action in actions) {
            if (attacks.TryGetValue(action, out var attack)) attack.Disable();
        }
    }

    public void EnableAttacks(params PlayerAction[] actions) {
        foreach (var action in actions) {
            if (attacks.TryGetValue(action, out var attack)) attack.Enable();
        }
    }

    public void SpendPower(int amount) {
        var powerOverflow = Math.Max((Timer.TotalPowerGeneration + powerGainedFromGrazing - powerSpent) - 400, 0);
        powerSpent += powerOverflow + amount;
    }

    public void SpawnProjectile(Projectile projectile) {
        projectile.SetId(totalSpawnedProjectiles);

        projectile.Destroyed += () => projectiles.Remove(projectile.Id);
        projectiles.Add(projectile.Id, projectile);
        Scene.AddEntity(projectile);

        totalSpawnedProjectiles++;
    }

    private void ChangeVelocity(Vector2f newVelocity) {
        if (newVelocity == Velocity) return;
        Velocity = newVelocity;
        var packet = new Packet(PacketType.VelocityChange);
        packet.In((long)Game.Network.Time).In(Position).In(Velocity);
        Game.Network.Send(packet);
    }

    private void RenderCooldowns() {
        var rect = new RectangleShape(new Vector2f(48f, 48f));

        var text = new Text();
        text.Font = Game.DefaultFont;
        text.FillColor = Color.Black;
        text.CharacterSize = 14;

        int offset = 0;

        foreach (var (name, attack) in attacks) {

            text.DisplayedString = name.ToString();

            rect.FillColor = Power < attack.Cost ? new Color(230, 180, 190) : Color.White;
            rect.Position = new Vector2f(4f + offset * 52f, Game.Window.Size.Y - 54f);
            rect.Size = new Vector2f(48f, 48f);
            rect.Origin = new Vector2f(0f, rect.Size.Y);

            cooldownShader.SetUniform("texture", Shader.CurrentTexture);
            cooldownShader.SetUniform("duration", attack.Cooldown.AsSeconds() / attack.CooldownDuration.AsSeconds());
            cooldownShader.SetUniform("position", new Vector2f(rect.Position.X, Game.Window.Size.Y - rect.Position.Y));
            cooldownShader.SetUniform("size", rect.Size);
            Game.Window.Draw(rect, new RenderStates(cooldownShader));

            text.Position = new Vector2f(4f + offset * 52f, Game.Window.Size.Y - 54f - rect.Size.Y);
            Game.Window.Draw(text);

            rect.FillColor = new Color(0, 0, 0, 80);
            if (attack.Disabled) {
                Game.Window.Draw(rect);
            }
            //else if (attack.Cooldown > 0) {
            //     rect.Size = new Vector2f(48f, attack.Cooldown.AsSeconds() / attack.CooldownDuration.AsSeconds() * 48f);
            //     rect.Origin = new Vector2f(0f, rect.Size.Y);
            //     Game.Window.Draw(rect);
            // }

            offset++;
        }
    }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        switch (packet.Type) {
            case PacketType.DestroyProjectile:
                packet.Out(out int id);
                if (projectiles.TryGetValue(id, out var projectile)) projectile.Destroy();
                break;
        }
    }

    protected void AddAttack(PlayerAction action, Attack attack) {
        attacks[action] = attack;
    }
}