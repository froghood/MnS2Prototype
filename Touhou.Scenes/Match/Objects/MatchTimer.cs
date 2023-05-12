using SFML.Graphics;
using SFML.System;
using Touhou.Objects;

namespace Touhou.Scenes.Match.Objects;

public class MatchTimer : Entity {

    public Time StartTime { get; }
    public Time CurrentReal { get => Game.Network.Time - StartTime; }
    public Time Current { get; private set; }

    public int Power {
        get {
            float p1 = 20 * MathF.Min(MathF.Max(MathF.Floor(Current.AsSeconds() * 5) / 5 - 0, 0f), 30f);
            float p2 = 25 * MathF.Min(MathF.Max(MathF.Floor(Current.AsSeconds() * 5) / 5 - 30, 0f), 20f);
            float p3 = 35 * MathF.Min(MathF.Max(MathF.Floor(Current.AsSeconds() * 5) / 5 - 50, 0f), 20f);
            float p4 = 50 * MathF.Min(MathF.Max(MathF.Floor(Current.AsSeconds() * 5) / 5 - 70, 0f), 20f);
            float p5 = 70 * MathF.Max(MathF.Floor(Current.AsSeconds() * 5) / 5 - 90, 0f);

            return (int)MathF.Round(p1 + p2 + p3 + p4 + p5);
        }
    }

    private Text text = new();

    public MatchTimer(Time startTime) {
        StartTime = startTime;

        text.Font = Game.DefaultFont;
        text.CharacterSize = 20;
    }

    public override void Update() {
        Current = Math.Max(Current, CurrentReal);
    }

    public override void Render() {
        text.DisplayedString = Current.AsSeconds().ToString();
        var bounds = text.GetLocalBounds();
        text.Origin = new Vector2f(bounds.Width * 0.5f, 0f);
        text.Position = new Vector2f(Game.Window.Size.X * 0.5f, 0f + 30f);

        Game.Window.Draw(text);
    }
    public override void PostRender() { }
}