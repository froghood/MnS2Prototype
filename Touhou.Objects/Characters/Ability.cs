namespace Touhou.Objects.Characters;

public class Ability {

    public bool IsAvailable => !IsDisabled && LockTimer.HasFinished && CooldownTimer.HasFinished;



    public Timer LockTimer { get; private set; }
    public Timer CooldownTimer { get; private set; }
    public bool IsDisabled { get; private set; }

    private Attack[] attacks;

    public Ability(Attack lvl1, Attack lvl2, Attack lvl3) {
        attacks = new Attack[] { lvl1, lvl2, lvl3 };
    }

    public void ApplyLock(Time duration) => LockTimer = new Timer(duration);
    public void ApplyCooldown(Time duration) {
        if (CooldownTimer.Remaining < duration) CooldownTimer = new Timer(duration);
    }

    public void Disable() => IsDisabled = true;
    public void Enable() => IsDisabled = false;

    public Attack GetAttack(int charge) => attacks[charge];



}