

// using OpenTK.Mathematics;
// using Touhou.Graphics;

// namespace Touhou.Objects;

// public class DistributionGraph : Entity {

//     public Vector2 Size { get; set; }
//     private Dictionary<string, (List<float> Samples, Func<float> ValueDelegate, int MaxSamples, Color4 Color4)> graphs = new();

//     private Rectangle rectangle;
//     private Text leftText;
//     private Text middleText;
//     private Text rightText;

//     public DistributionGraph() {

//         rectangle = new Rectangle() {
//             Size = Size,
//             Origin = new Vector2(0f, 0f),
//             Position = Position,
//             FillColor = new Color4(0, 0, 0, 180),
//             StrokeWidth = 1f,
//             StrokeColor = new Color4(255, 255, 255, 60),
//             UIAlignment = new Vector2(0f, 1f), // middle top
//         };

//         leftText = new Text();
//         middleText = new Text();
//         rightText = new Text();
//     }


//     public void Add(string label, Func<float> valueDelegate, int maxSamples, Color4 Color4) {
//         graphs.Add(label, (new List<float>(), valueDelegate, maxSamples, Color4));
//     }

//     public void Sample(string label, float value) {
//         if (graphs.TryGetValue(label, out var data)) {
//             data.Samples.Add(value);
//         }
//     }

//     public override void Update() {
//         foreach (var graph in graphs.Values) {
//             if (graph.ValueDelegate != null) graph.Samples.Add(graph.ValueDelegate.Invoke());
//             while (graph.Samples.Count > graph.MaxSamples) graph.Samples.RemoveAt(0);
//         }
//     }

//     public override void Render() {

//         // background
//         rectangle.Size = Size;
//         rectangle.Origin = new Vector2(0f, 0f);
//         rectangle.Position = Position;
//         rectangle.FillColor = new Color4(0, 0, 0, 180);
//         rectangle.StrokeWidth = 1f;
//         rectangle.StrokeColor = new Color4(255, 255, 255, 60);

//         Game.Draw(rectangle);



//         float largestSample = 0f;
//         foreach (var graph in graphs.Values) {
//             foreach (var sample in graph.Samples) {
//                 var absSample = MathF.Abs(sample);
//                 if (absSample > largestSample) largestSample = absSample;
//             }
//         }

//         leftText.DisplayedText = $"{-largestSample}";
//         leftText.Origin = new Vector2(0f, 0f);
//         leftText.Position = Position + new Vector2(Size.X / 2f, 0f);
//         Game.Draw(leftText);

//         middleText.DisplayedText = "0";
//         middleText.Origin = new Vector2(0.5f, 0f);
//         middleText.Position = Position + new Vector2(Size.X, 0f);
//         Game.Draw(middleText);

//         rightText.DisplayedText = $"{largestSample}";
//         rightText.Origin = new Vector2(1, 0f);
//         rightText.Position = Position;
//         //Game.Draw(text, 0);


//         // samples
//         rectangle.Size = new Vector2(2f, 8f);
//         rectangle.Origin = rectangle.Size / 2f;
//         rectangle.FillColor = new Color4(0, 255, 0, 50);
//         rectangle.StrokeWidth = 0f;

//         foreach (var graph in graphs.Values) {
//             foreach (var sample in graph.Samples) {
//                 rectangle.Position = Position + new Vector2() {
//                     X = Size.X / 2f + sample / largestSample * Size.X / 2f,
//                     Y = Size.Y * 0.75f
//                 };

//                 //Game.Draw(rectangle, 0);
//             }
//         }

//         // averages
//         rectangle.FillColor = new Color4(0, 100, 255, 200);
//         rectangle.Position = Position + new Vector2() {
//             X = Size.X / 2f + GetAverage(graphs.First().Value) / largestSample * Size.X / 2f,
//             Y = Size.Y * 0.25f
//         };
//         //Game.Draw(rectangle, 0);

//         rectangle.FillColor = new Color4(255, 200, 0, 200);
//         rectangle.Position = Position + new Vector2() {
//             X = Size.X / 2f + GetPrunedAverage(graphs.First().Value, 50) / largestSample * Size.X / 2f,
//             Y = Size.Y * 0.25f
//         };
//         //Game.Draw(rectangle, 0);



//         rectangle.Size = new Vector2(2f, 2f);
//         rectangle.Origin = rectangle.Size / 2f;
//         rectangle.FillColor = new Color4(255, 255, 255, 80);

//         rectangle.Position = Position + new Vector2() {
//             X = Size.X / 2f,
//             Y = Size.Y - 4f
//         };

//         //Game.Draw(rectangle, 0);

//         int markerCount = 1;
//         while (markerCount < largestSample) {
//             rectangle.Position = Position + new Vector2() {
//                 X = Size.X / 2f + markerCount / largestSample * Size.X / 2f,
//                 Y = Size.Y - 2f
//             };

//             //Game.Draw(rectangle, 0);

//             rectangle.Position = Position + new Vector2() {
//                 X = Size.X / 2f + -markerCount / largestSample * Size.X / 2f,
//                 Y = Size.Y - 2f
//             };

//             //Game.Draw(rectangle, 0);

//             markerCount++;
//         }


//     }

//     public override void PostRender() {

//     }

//     private float GetAverage((List<float> Samples, Func<float> ValueDelegate, int MaxSamples, Color4 Color4) graph) {
//         float total = 0f;
//         foreach (var sample in graph.Samples) {
//             total += sample;
//         }

//         return total / graph.Samples.Count;
//     }

//     private float GetPrunedAverage((List<float> Samples, Func<float> ValueDelegate, int MaxSamples, Color4 Color4) graph, int numToPrune) {

//         float total = 0f;
//         int count = graph.Samples.Count;
//         foreach (var sample in graph.Samples) {
//             total += sample;
//         }

//         if (count <= numToPrune) {
//             return total / count;
//         }

//         var prunedSamples = new HashSet<int>();

//         for (int n = 0; n < numToPrune; n++) {

//             var influences = new List<(float InfleunceAmount, int Index)>();
//             float average = total / count;

//             for (int i = 0; i < graph.Samples.Count; i++) {
//                 if (prunedSamples.Contains(i)) continue;
//                 float sample = graph.Samples[i];

//                 float influenceAmount = MathF.Abs(average - (total - sample) / (count - 1));
//                 influences.Add((influenceAmount, i));

//             }

//             var largestInfluence = influences.MaxBy(e => e.InfleunceAmount);
//             prunedSamples.Add(largestInfluence.Index);

//             total -= graph.Samples[largestInfluence.Index];
//             count--;
//         }

//         return total / count;
//     }

// }