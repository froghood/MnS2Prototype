namespace Touhou.Objects.Characters;

public abstract class Effect {

    public Time LifeTime { get => timer.TotalElapsed; }
    public bool HasTimedOut { get => timer.IsFinished; }
    public bool IsCanceled { get => isCanceled; }



    private Timer timer;
    private bool isCanceled;

    public Effect(Time duration) {
        this.timer = new Timer(duration);
    }

    public abstract void PlayerUpdate(Player player);
    public abstract void OpponentUpdate(Opponent opponent);

    public virtual void Cancel(Time time = default) {
        isCanceled = true;
    }
}