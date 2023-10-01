namespace Touhou.Objects.Characters;

public abstract class Effect {

    public Time LifeTime { get => Game.Time - creationTime; }
    public bool HasTimedOut { get => LifeTime >= duration; }
    public bool IsCanceled { get => isCanceled; }



    private Time duration;
    private Time creationTime;
    private bool isCanceled;

    public Effect(Time duration) {
        this.duration = duration;
        this.creationTime = Game.Time;
    }

    public abstract void PlayerUpdate(Player player);
    public abstract void OpponentUpdate(Opponent opponent);

    public virtual void Cancel(Time timeOverride = default) {
        isCanceled = true;
    }
}