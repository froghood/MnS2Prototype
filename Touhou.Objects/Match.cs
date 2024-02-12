using OpenTK.Mathematics;
using Touhou.Objects.Characters;

namespace Touhou.Objects;

public abstract class Match : Entity {


    public Time StartTime { get; protected set; }
    public Time EndTime { get; protected set; }


    public bool HasStarted { get => CurrentTime >= 0L; }

    public Vector2 Bounds { get => new Vector2(736f, 368f); }

    public abstract Time CurrentTime { get; protected set; }


    public bool IsP1 { get; private set; }

    private List<(Time Time, int Increase)> powerGenerationIncreaseBreakpoints = new() {
        (Time.InSeconds(0f), 8),
        (Time.InSeconds(49f), 8),
        (Time.InSeconds(99f), 16),
    };

    public int TotalPowerGenerated {
        get {
            float powerGen = 0f;
            foreach (var breakpoint in powerGenerationIncreaseBreakpoints) {
                powerGen += MathF.Max(MathF.Floor((CurrentTime - breakpoint.Time).AsSeconds() * 5f) / 5f, 0f) * breakpoint.Increase;
            }

            return (int)MathF.Round(powerGen);
        }
    }

    public int CurrentPowerPerSecond {
        get {
            int powerPerSecond = 0;
            foreach (var breakpoint in powerGenerationIncreaseBreakpoints) {
                if (CurrentTime >= breakpoint.Time) powerPerSecond += breakpoint.Increase;
            }
            return powerPerSecond;
        }
    }


    public Match(bool isP1, Time startTime) {
        IsP1 = isP1;

        StartTime = startTime;
        EndTime = startTime + Time.InSeconds(99f);
    }
}
