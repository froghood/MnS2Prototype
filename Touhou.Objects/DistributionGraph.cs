
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

    public override void Update() {
        foreach (var graph in graphs.Values) {
            if (graph.ValueDelegate != null) graph.Samples.Add(graph.ValueDelegate.Invoke());
            while (graph.Samples.Count > graph.MaxSamples) graph.Samples.RemoveAt(0);
        }
    }

    public override void Render() {

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

        // averages
        rectangle.FillColor = new Color(0, 100, 255, 200);
        rectangle.Position = Position + new Vector2f() {
            X = Size.X / 2f + GetAverage(graphs.First().Value) / largestSample * Size.X / 2f,
            Y = Size.Y * 0.25f
        };
        Game.Window.Draw(rectangle);

        rectangle.FillColor = new Color(255, 200, 0, 200);
        rectangle.Position = Position + new Vector2f() {
            X = Size.X / 2f + GetPrunedAverage(graphs.First().Value, 50) / largestSample * Size.X / 2f,
            Y = Size.Y * 0.25f
        };
        Game.Window.Draw(rectangle);



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

    public override void PostRender() {

    }

    private float GetAverage((List<float> Samples, Func<float> ValueDelegate, int MaxSamples, Color Color) graph) {
        float total = 0f;
        foreach (var sample in graph.Samples) {
            total += sample;
        }

        return total / graph.Samples.Count;
    }

    private float GetPrunedAverage((List<float> Samples, Func<float> ValueDelegate, int MaxSamples, Color Color) graph, int numToPrune) {

        float total = 0f;
        int count = graph.Samples.Count;
        foreach (var sample in graph.Samples) {
            total += sample;
        }

        if (count <= numToPrune) {
            return total / count;
        }

        var prunedSamples = new HashSet<int>();

        for (int n = 0; n < numToPrune; n++) {

            var influences = new List<(float InfleunceAmount, int Index)>();
            float average = total / count;

            for (int i = 0; i < graph.Samples.Count; i++) {
                if (prunedSamples.Contains(i)) continue;
                float sample = graph.Samples[i];

                float influenceAmount = MathF.Abs(average - (total - sample) / (count - 1));
                influences.Add((influenceAmount, i));

            }

            var largestInfluence = influences.MaxBy(e => e.InfleunceAmount);
            prunedSamples.Add(largestInfluence.Index);

            total -= graph.Samples[largestInfluence.Index];
            count--;
        }

        return total / count;
    }

}