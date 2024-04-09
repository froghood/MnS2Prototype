

using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;



public class LocalCharacterController<T> : Entity where T : Character {


    private T c;

    private Vector2 previousVelocity;


    private Vector2 knockbackStartPosition;
    private Vector2 knockbackEndPosition;

    public LocalCharacterController(T c) {

        this.c = c;

        c.CanCollide = true;

        c.Hitboxes.Add(new CircleHitbox(c, Vector2.Zero, 0.5f, c.IsP1 ? CollisionGroup.P1 : CollisionGroup.P2, Hit));
        c.Hitboxes.Add(new CircleHitbox(c, Vector2.Zero, 50f, c.IsP1 ? CollisionGroup.P1 : CollisionGroup.P2, Grazed));


    }



    public override void Update() {

        if (!c.Match.HasStarted || c.State == CharacterState.Dead) return;

        if (c.State == CharacterState.Free) UpdateMovement();
        if (c.State == CharacterState.Hit) UpdateHit();
        if (c.State == CharacterState.Knockbacked) {
            UpdateKnockback();
            return;
        }

        foreach (var action in Game.Input.GetActionOrder().Where(e => (e & (PlayerActions.Primary | PlayerActions.Secondary | PlayerActions.Special | PlayerActions.Super | PlayerActions.Bomb)) != 0)) {
            UpdateAttackPress(action);
            UpdateBomb(action);
            UpdateAttackHold(action);
        }
    }



    private void UpdateMovement() {

        // movement
        if (Game.IsActionPressed(PlayerActions.Focus) && !c.IsFocused) c.Focus();
        if (!Game.IsActionPressed(PlayerActions.Focus) && c.IsFocused) c.Unfocus();

        var inputVector = new Vector2(
            (Game.IsActionPressed(PlayerActions.Right) ? 1f : 0f) - (Game.IsActionPressed(PlayerActions.Left) ? 1f : 0f),
            (Game.IsActionPressed(PlayerActions.Up) ? 1f : 0f) - (Game.IsActionPressed(PlayerActions.Down) ? 1f : 0f)
        );

        var movementAngle = MathF.Atan2(inputVector.Y, inputVector.X);
        var movementVector = new Vector2(MathF.Abs(MathF.Cos(movementAngle)), MathF.Abs(MathF.Sin(movementAngle))) * inputVector;


        var speed = c.IsFocused ? c.FocusedSpeed : c.Speed;
        var modifier = c.MovespeedModifierTimer.HasFinished ? 1f : c.MovespeedModifier;

        var velocity = movementVector * speed * modifier;

        //Log.Info($"{movementVector}, {speed}, {modifier}, {velocity}");

        if (velocity != previousVelocity) {

            c.SetVelocity(velocity);

            previousVelocity = velocity;

            Game.NetworkOld.Send(
                PacketType.VelocityChanged,
                Game.NetworkOld.Time,
                c.Position,
                c.Velocity);
        }

        // output
        c.SetPosition(c.Position + c.Velocity * Game.Delta.AsSeconds());

        // keeps character inside match bounds
        c.SetPosition(Vector2.Clamp(c.Position, -c.Match.Bounds, c.Match.Bounds));
    }



    private void UpdateHit() {

        if (!c.HitTimer.HasFinished) return;

        // when finished

        c.Damage();

        Knockback();

        if (c.HeartCount == 1) Game.Sounds.Play("low_hearts");
        if (c.HeartCount <= 0) {
            Die();
            return;
        }

        Game.NetworkOld.Send(
            PacketType.Knockbacked,
            Game.NetworkOld.Time,
            c.Position,
            c.AngleToOpponent);

    }



    private void UpdateKnockback() {
        if (c.KnockbackedTimer.HasFinished) {
            c.SetState(CharacterState.Free);
            return;
        }

        float t = 1f - c.KnockbackedTimer.RemainingRatio;
        c.SetPosition(knockbackStartPosition + (knockbackEndPosition - knockbackStartPosition) * Easing.Out(t, 5f));

        c.SetPosition(Vector2.Clamp(c.Position, -c.Match.Bounds, c.Match.Bounds));
    }

    private void UpdateAttackPress(PlayerActions action) {


        if (action == PlayerActions.Bomb) return;


        if (c.State == CharacterState.Hit) return;


        if (!c.IsAttackAvailable(action)) return;

        if (c.IsAttackHoldable(action) && !c.IsAttackHeld(action)) { // holdable
            ProcessHoldable(action);
        }

        if (!c.IsAttackHoldable(action)) { // not holdable
            ProcessNonHoldable(action);
        }




        void ProcessHoldable(PlayerActions action) {

            var attackFinishTime = c.GetAttackCooldownTimer(action).FinishTime;

            if (Game.Input.IsActionPressBuffered(action, out var pressBufferTime, out var bufferState)) {
                Game.Input.ConsumePressBuffer(action);

                bool focused = bufferState.HasFlag(PlayerActions.Focus);

                var cooldownOverflow = default(Time);
                var heldTime = default(Time);



                if (Game.IsActionPressed(action)) {

                    heldTime = pressBufferTime < attackFinishTime ? attackFinishTime : Game.Time;

                } else {

                    var releaseTime = Game.Input.GetLastReleaseTime(action);

                    heldTime = releaseTime >= attackFinishTime ? attackFinishTime : Game.Time;
                    cooldownOverflow = releaseTime < attackFinishTime ? Game.Time - attackFinishTime : (Time)0L;

                }

                Log.Info($"Buffered action {action} | cooldown overflow: {cooldownOverflow}μs | held time offset: {Game.Time - heldTime}");

                c.PressLocalAttack(action, cooldownOverflow, heldTime, focused);

            } else {

                bool focused = Game.IsActionPressed(PlayerActions.Focus);

                var heldTime = (Game.Time - Game.Delta) > attackFinishTime ? Game.Time : attackFinishTime;

                if (Game.IsActionPressed(action)) {

                    Log.Info($"Held action {action} | cooldown overflow: {0L}μs | held time offset: {Game.Time - heldTime}");

                } else {

                    var releaseTime = Game.Input.GetLastReleaseTime(action);

                    // niche case for when a release happens on the next available frame after the attack's cooldown ends
                    // must check if the cooldown finish time happened inbetween the current and previous frame times;
                    // required in order to stop unintended activations before the attack's cooldown timer has been set for the first time
                    if (releaseTime < attackFinishTime || (Game.Time - Game.Delta) > attackFinishTime) return;

                    Log.Warn($"Released action {action} | cooldown overflow: {0L}μs | held time offset: {Game.Time - heldTime}");

                }

                c.PressLocalAttack(action, 0L, heldTime, focused);
            }
        }



        void ProcessNonHoldable(PlayerActions action) {
            if (!Game.Input.IsActionPressBuffered(action, out var bufferPressTime, out var bufferState)) return;
            Game.Input.ConsumePressBuffer(action);

            var attackFinishTime = c.GetAttackCooldownTimer(action).FinishTime;

            var cooldownOverflow = (bufferPressTime < attackFinishTime) ?
                Game.Time - attackFinishTime : (Time)0L;

            c.PressLocalAttack(action, cooldownOverflow, bufferState.HasFlag(PlayerActions.Focus));
        }
    }

    private void UpdateAttackHold(PlayerActions action) {

        if (action != PlayerActions.Primary && action != PlayerActions.Secondary && action != PlayerActions.Special && action != PlayerActions.Super) return;


        if (c.GetAttackCooldownTimer(action).HasFinished) c.HoldLocalAttack(action);

        if (!Game.IsActionPressed(action) || c.State != CharacterState.Free) c.ReleaseLocalAttack(action);
    }



    private void UpdateBomb(PlayerActions action) {


        if (action != PlayerActions.Bomb || !c.IsBombAvailable()) return;



        if (!Game.Input.IsActionPressBuffered(PlayerActions.Bomb, out var bufferPressTime, out var bufferState)) return;

        Game.Input.ConsumePressBuffer(PlayerActions.Bomb);

        var bombFinishTime = c.GetBombCooldownTimer().FinishTime;

        var cooldownOverflow = (bufferPressTime < bombFinishTime) ?
            Game.Time - bombFinishTime : (Time)0L;

        c.ReleaseAllLocalAttacks();
        c.PressLocalBomb(cooldownOverflow, bufferState.HasFlag(PlayerActions.Focus));
        c.SetState(CharacterState.Free);

        Game.Sounds.Play("spell");
        Game.Sounds.Play("bomb");
    }

    private void Grazed(Entity entity, Hitbox hitbox) {
        if (entity is not Projectile projectile) return;

        if (c.State != CharacterState.Free || projectile.Grazed) return;

        projectile.Graze();
        c.Graze(projectile.GrazeAmount);

        Game.NetworkOld.Send(PacketType.Grazed, projectile.GrazeAmount);
    }

    private void Hit(Entity entity, Hitbox hitbox) {

        if (entity is not Projectile projectile) return;

        if (c.IsInvulnerable) return;

        c.ApplyInvulnerability(Time.InSeconds(2.5f));
        c.Hit(Time.InSeconds(8f / 60f)); // 8 frames

        Scene.AddEntity(new HitExplosion(c.Position, 0.5f, 100f, c.Color));

        Game.Sounds.Play("hit");

        Game.NetworkOld.Send(PacketType.Hit, Game.NetworkOld.Time, c.Position);

        if (hitbox.CollisionGroup == CollisionGroup.P1MinorProjectile ||
            hitbox.CollisionGroup == CollisionGroup.P2MinorProjectile)
            projectile.NetworkDestroy();

    }

    private void Knockback() {
        c.Knockback(Time.InSeconds(1f));

        float angle = c.AngleToOpponent + MathF.PI;
        float strength = 100f;

        knockbackStartPosition = c.Position;
        knockbackEndPosition = c.Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * strength;
    }



    private void Die() {

        var deathTime = Game.NetworkOld.IsConnected ? Game.NetworkOld.Time : Game.Time;

        c.ApplyInvulnerability();
        c.Die(deathTime);

        Scene.AddEntity(new HitExplosion(c.Position, 1f, 500f, c.Color));

        Game.Sounds.Play("death");

        Game.NetworkOld.Send(PacketType.Death, deathTime, c.Position);


    }
}