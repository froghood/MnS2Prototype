namespace Touhou;

public struct Timer {

    public Time StartTime { get => startTime; }
    public Time Duration { get => duration; }
    public Time FinishTime { get => finishTime; }
    public Time TotalElapsed { get => Game.Time - startTime; }
    public Time Remaining { get => Time.Max(finishTime - Game.Time, 0L); }
    public bool HasFinished { get => Game.Time >= finishTime; }

    private Time startTime { get; init; }
    private Time duration { get; init; }
    private Time finishTime { get; init; }

    public Timer(Time duration) {
        startTime = Game.Time;
        this.duration = duration;
        finishTime = startTime + duration;

    }

    public Timer() {
        startTime = Game.Time;
        duration = 0L;
        startTime = Game.Time;
    }

    public static Timer Max() {

        return new Timer {
            startTime = Game.Time,
            duration = long.MaxValue - Game.Time,
            finishTime = long.MaxValue
        };

    }
}