
using SFML.Graphics;
using SFML.System;

namespace Touhou.Objects;

public class DistributionGraph : Entity {

    public Vector2f Size { get; set; }
    private Dictionary<string, (List<float> Samples, Func<float> ValueDelegate, int MaxSamples, Color Color)> graphs = new();

    private RectangleShape rectangle;
    private Text text;

    public DistributionGraph() {

        rectangle = new RectangleShape();

        text = new Text();
        text.Font = Game.DefaultFont;
        text.CharacterSize = 12;
    }


    public void Add(string label, Func<float> valueDelegate, int maxSamples, Color color) {
        graphs.Add(label, (new List<float>(), valueDelegate, maxSamples, color));
    }

    public void Sample(string label, float value) {
        if (graphs.TryGetValue(label, out var data)) {
            data.Samples.Add(value);
        }
    }

    public override void Update(Time time, float delta) {
        foreach (var graph in graphs.Values) {
            if (graph.ValueDelegate != null) graph.Samples.Add(graph.ValueDelegate.Invoke());
            while (graph.Samples.Count > graph.MaxSamples) graph.Samples.RemoveAt(0);
        }
    }

    public override void Render(Time time, float delta) {

        // background
        rectangle.Size = Size;
        rectangle.Origin = new Vector2f(0f, 0f);
        rectangle.Position = Position;
        rectangle.FillColor = new Color(0, 0, 0, 180);
        rectangle.OutlineThickness = 1f;
        rectangle.OutlineColor = new Color(255, 255, 255, 60);
        Game.Window.Draw(rectangle);



        float largestSample = 0f;
        foreach (var graph in graphs.Values) {
            foreach (var sample in graph.Samples) {
                var absSample = MathF.Abs(sample);
                if (absSample > largestSample) largestSample = absSample;
            }
        }

        text.DisplayedString = "0";
        text.Origin = new Vector2f(text.GetLocalBounds().Width / 2f, 0f);
        text.Position = Position + new Vector2f(Size.X / 2f, 0f);
        Game.Window.Draw(text);

        text.DisplayedString = $"{largestSample}";
        text.Origin = new Vector2f(text.GetLocalBounds().Width, 0f);
        text.Position = Position + new Vector2f(Size.X, 0f);
        Game.Window.Draw(text);

        text.DisplayedString = $"{-largestSample}";
        text.Origin = new Vector2f(0, 0f);
        text.Position = Position;
        Game.Window.Draw(text);


        // samples
        rectangle.Size = new Vector2f(2f, 8f);
        rectangle.Origin = rectangle.Size / 2f;
        rectangle.FillColor = new Color(0, 255, 0, 50);
        rectangle.OutlineThickness = 0f;

        foreach (var graph in graphs.Values) {
            foreach (var sample in graph.Samples) {
                rectangle.Position = Position + new Vector2f() {
                    X = Size.X / 2f + sample / largestSample * Size.X / 2f,
                    Y = Size.Y * 0.75f
                };

                Game.Window.Draw(rectangle);
            }
        }



        rectangle.Size = new Vector2f(2f, 2f);
        rectangle.Origin = rectangle.Size / 2f;
        rectangle.FillColor = new Color(255, 255, 255, 80);

        rectangle.Position = Position + new Vector2f() {
            X = Size.X / 2f,
            Y = Size.Y - 4f
        };

        Game.Window.Draw(rectangle);

        int markerCount = 1;
        while (markerCount < largestSample) {
            rectangle.Position = Position + new Vector2f() {
                X = Size.X / 2f + markerCount / largestSample * Size.X / 2f,
                Y = Size.Y - 2f
            };

            Game.Window.Draw(rectangle);

            rectangle.Position = Position + new Vector2f() {
                X = Size.X / 2f + -markerCount / largestSample * Size.X / 2f,
                Y = Size.Y - 2f
            };

            Game.Window.Draw(rectangle);

            markerCount++;
        }


    }

    public override void Finalize(Time time, float delta) {

    }



}