namespace Touhou;

public struct Timer {

    public Time StartTime { get => startTime; }
    public Time Amount { get => amount; }
    public Time TotalElapsed { get => Game.Time - startTime; }
    public Time Remaining { get => Time.Max(startTime + amount - Game.Time, 0L); }
    public bool IsFinished { get => Remaining == 0L; }

    private Time startTime;
    private Time amount;

    public Timer(Time amount) {
        startTime = Game.Time;
        this.amount = amount;
    }
}