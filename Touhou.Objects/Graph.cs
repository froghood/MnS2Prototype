// using System.Collections;
// using System.Numerics;


// namespace Touhou.Objects;

// public class Graph : Entity {

//     public Vector2 Size { get; set; }
//     private Dictionary<string, (List<float> Samples, Func<float> ValueDelegate, int MaxSamples, Color4 Color4)> graphs = new();


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

//         var bg = new RectangleShape(Size);
//         bg.Position = Position;
//         bg.FillColor4 = new Color4(0, 0, 0, 80);
//         //Game.Draw(bg, 0);


//         float largestSample = 0f;
//         foreach (var graph in graphs.Values) {
//             foreach (var sample in graph.Samples) {
//                 if (sample > largestSample) largestSample = sample;
//             }
//         }

//         foreach (var graph in graphs.Values) {
//             var vertexArray = new Vertex[graph.Samples.Count];
//             for (int i = 0; i < graph.Samples.Count; i++) {
//                 vertexArray[i] = new Vertex(new Vector2(
//                     Position.X + (float)i / graph.MaxSamples * Size.X,
//                     Position.Y + Size.Y - (graph.Samples[i] / largestSample * Size.Y)
//                 ), graph.Color4);
//             }
//             //Game.Window.Draw(vertexArray, PrimitiveType.LineStrip);
//         }
//     }

//     public override void PostRender() {
//     }
//     // public Func<float> Track { get => _track; }
//     // public uint SampleCount { get; set; }
//     // public float[] Samples { get => _samples; set => _samples = value; }
//     // public float MaxSample { get; set; }
//     // public Color4 BackgroundColor4 { get => _background.FillColor4; set => _background.FillColor4 = value; }
//     // public Color4 LineColor4 { get => _line.FillColor4; set => _line.FillColor4 = value; }
//     // public Vector2 Position { get => _background.Position; set => _background.Position = value; }
//     // public Vector2 Size { get => _background.Size; set => _background.Size = value; }




//     // private Func<float> _track;

//     // private Queue<float> _queue = new();
//     // private float[] _samples;

//     // private RectangleShape _background = new();
//     // private RectangleShape _line = new();

//     // public Graph() {

//     // }

//     // public Graph(Func<float> track) {
//     //     _track += track;
//     // }

//     // public void Update(float[] samples) {
//     //     _samples = samples;
//     //     // _queue.Enqueue(_track.Invoke());
//     //     // while (_queue.Count > SampleCount && _queue.TryDequeue(out var overflow)) ;
//     // }


//     // public void Draw(RenderTarget target, RenderStates states) {
//     //     _background.Draw(target, states);

//     //     var vertexArray = new VertexArray(PrimitiveType.LineStrip, SampleCount);
//     //     vertexArray.Clear();

//     //     int i = 0;
//     //     foreach (float n in _samples) {

//     //         vertexArray.Append(new Vertex(
//     //             Position + new Vector2(Size.X / (SampleCount - 1) * i, Size.Y - n / MaxSample * Size.Y),
//     //             LineColor4
//     //         ));
//     //         i++;
//     //     }

//     //     vertexArray.Draw(target, states);

//     // }

// }

// public class GraphHead<T> : IEnumerable<T> {


//     public GraphNode<T> First { get; private set; }
//     public GraphNode<T> Last { get; private set; }

//     public int Count { get => count; }
//     private int count;

//     public void Add(T value) {
//         if (count == 0) {
//             var node = new GraphNode<T>(value);
//             First = node;
//             Last = node;
//         } else {
//             Last.Extend(value);
//             Last = Last.Next;
//         }
//         count++;
//     }

//     public void Remove() {
//         if (count > 1) First = First.Next;
//         else if (count > 0) First = null;
//         else return;
//         count--;
//     }

//     public IEnumerator<T> GetEnumerator() {
//         var node = First;
//         while (node != null) {
//             yield return node.Value;
//             node = node.Next;
//         }
//     }

//     IEnumerator IEnumerable.GetEnumerator() {
//         return GetEnumerator();
//     }
// }

// public class GraphNode<T> {
//     public T Value { get; private set; }
//     public GraphNode<T> Next { get; private set; }

//     public GraphNode(T value) {
//         Value = value;
//     }

//     public void Extend(T value) {
//         Next = new GraphNode<T>(value);
//     }
// }