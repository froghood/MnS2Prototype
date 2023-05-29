using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Objects;

public class MatchTimer : Entity {

    public Time StartTime { get; }
    public Time EndTime { get; }
    public Time CurrentReal { get => Game.Network.Time - StartTime; }
    public Time Current { get; private set; }

    public bool MatchStarted { get; private set; }

    private Text text = new();

    private List<(Time Time, int Increase)> powerGenerationIncreaseBreakpoints = new() {
        (Time.InSeconds(0f), 8),
        (Time.InSeconds(49f), 8),
        (Time.InSeconds(99f), 16)
    };

    public int TotalPowerGenerated {
        get {
            float powerGen = 0f;
            foreach (var breakpoint in powerGenerationIncreaseBreakpoints) {
                powerGen += MathF.Max(MathF.Floor((Current - breakpoint.Time).AsSeconds() * 5f) / 5f, 0f) * breakpoint.Increase;
            }

            return (int)MathF.Round(powerGen);
        }
    }



    public MatchTimer(Time startTime) {
        StartTime = startTime;
        EndTime = startTime + Time.InSeconds(99f);

        text.Font = Game.DefaultFont;
        text.CharacterSize = 20;
        text.Style = Text.Styles.Bold;
    }

    public override void Update() {
        Current = Math.Max(Current, CurrentReal);

        if (!MatchStarted && Game.Network.Time >= StartTime) {
            MatchStarted = true;

            Game.Network.Send(new Packet(PacketType.MatchStart));
        }
    }

    public override void Render() {
        var displayTime = MathF.Max(MathF.Ceiling((EndTime - StartTime - Current).AsSeconds()), 0f);
        text.DisplayedString = displayTime.ToString();
        text.CharacterSize = 20;
        text.Origin = new Vector2f(text.GetLocalBounds().Width * 0.5f, 0f);
        text.Position = new Vector2f(Game.Window.Size.X * 0.5f, 0f + 30f);
        Game.Window.Draw(text);

        int powerPerSecond = 0;
        foreach (var breakpoint in powerGenerationIncreaseBreakpoints) {
            if (Current >= breakpoint.Time) powerPerSecond += breakpoint.Increase;
        }

        text.DisplayedString = $"{powerPerSecond}/s";
        text.CharacterSize = 12;
        text.Origin = new Vector2f(text.GetLocalBounds().Width * 0.5f, 0f);
        text.Position = new Vector2f(Game.Window.Size.X * 0.5f, 0f + 55f);
        Game.Window.Draw(text);


    }
    public override void PostRender() { }
}