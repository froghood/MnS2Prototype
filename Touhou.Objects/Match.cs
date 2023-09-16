using OpenTK.Mathematics;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Objects;

public class Match : Entity {

    public Time StartTime { get; }
    public Time EndTime { get; }
    public Time CurrentTimeReal { get => Game.Network.Time - StartTime; }
    public Time CurrentTime { get; private set; }

    public bool Started { get; private set; }

    public Vector2 Bounds { get; private set; } = new(795f, 368f);

    //private Text text = new();

    public List<(Time Time, int Increase)> powerGenerationIncreaseBreakpoints = new() {
        (Time.InSeconds(0f), 8),
        (Time.InSeconds(49f), 8),
        (Time.InSeconds(99f), 16)
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



    public Match(Time startTime) {
        StartTime = startTime;
        EndTime = startTime + Time.InSeconds(99f);

        // text.Font = Game.DefaultFont;
        // text.CharacterSize = 20;
        // text.Style = Text.Styles.Bold;
    }

    public override void Update() {
        CurrentTime = Math.Max(CurrentTime, CurrentTimeReal);

        if (!Started && Game.Network.Time >= StartTime) {
            Started = true;

            Game.Network.Send(new Packet(PacketType.MatchStarted));
        }
    }

    public override void Render() {
        // var displayTime = MathF.Max(MathF.Ceiling((EndTime - StartTime - CurrentTime).AsSeconds()), 0f);
        // text.DisplayedString = displayTime.ToString();
        // text.CharacterSize = 20;
        // text.Origin = new Vector2(text.GetLocalBounds().Width * 0.5f, 0f);
        // text.Position = new Vector2(Game.Window.Size.X * 0.5f, 0f + 30f);
        // Game.Draw(text, 0);

        // int powerPerSecond = 0;
        // foreach (var breakpoint in powerGenerationIncreaseBreakpoints) {
        //     if (CurrentTime >= breakpoint.Time) powerPerSecond += breakpoint.Increase;
        // }

        // text.DisplayedString = $"{powerPerSecond}/s";
        // text.CharacterSize = 12;
        // text.Origin = new Vector2(text.GetLocalBounds().Width * 0.5f, 0f);
        // text.Position = new Vector2(Game.Window.Size.X * 0.5f, 0f + 55f);
        // Game.Draw(text, 0);


    }
    public override void PostRender() { }

    public int GetPowerPerSecond() {
        int powerPerSecond = 0;
        foreach (var breakpoint in powerGenerationIncreaseBreakpoints) {
            if (CurrentTime >= breakpoint.Time) powerPerSecond += breakpoint.Increase;
        }
        return powerPerSecond;
    }
}