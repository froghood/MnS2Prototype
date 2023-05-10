using SFML.Graphics;
using SFML.System;

namespace Touhou.Debugging;

public class Fields {


    private Dictionary<string, float> fields = new();

    public void Add(string name) => fields.Add(name, 0f);

    public bool TryGet(string name, out float value) {
        if (fields.TryGetValue(name, out float _value)) {
            value = _value;
            return true;
        } else {
            value = 0f;
            return false;
        }
    }

    public void Set(string name, float value) {
        if (!fields.ContainsKey(name)) return;
        fields[name] = value;
    }
}

// public class Stats : Drawable {

//     public Color BackgroundColor { get => _background.FillColor; set => _background.FillColor = value; }
//     public Vector2f Size { get => _background.Size; set => _background.Size = value; }
//     public Vector2f Position { get => _background.Position; set => _background.Position = value; }
//     public float TextSpacing { get; set; }

//     private Dictionary<string, (uint MaxSampleCount, float MaxSampleValue, Color Color, bool DisplayAverage, Queue<float> Samples)> _graphs = new();
//     private RectangleShape _background = new();
//     private Text _text = new();


//     public void AddGraph(string name, uint maxSampleCount, float maxValue, Color color, bool displayAverage) {
//         _graphs.Add(name, (maxSampleCount, maxValue, color, displayAverage, new Queue<float>()));
//     }

//     public void AddSample(string name, float value) {
//         _graphs.TryGetValue(name, out var graph);
//         graph.Samples.Enqueue(value);
//         while (graph.Samples.Count > graph.MaxSampleCount && graph.Samples.TryDequeue(out var overflow)) ;
//     }

//     public void Draw(RenderTarget target, RenderStates states) {

//         _background.Draw(target, states);

//         foreach (var (name, graph) in _graphs) {
//             var vertexArray = new VertexArray(PrimitiveType.LineStrip);

//             int index = 0;
//             foreach (float sample in graph.Samples) {
//                 vertexArray.Append(new Vertex(
//                     Position + new Vector2f(
//                         Size.X / (graph.MaxSampleCount - 1) * index,
//                         Size.Y - sample / graph.MaxSampleValue * Size.Y),
//                     graph.Color
//                 ));
//                 index++;
//             }

//             vertexArray.Draw(target, states);
//         }

//         float offset = 0f;
//         foreach (var (name, graph) in _graphs) {
//             _text.Font = Game.DefaultFont;
//             _text.CharacterSize = 14;
//             _text.FillColor = graph.Color;
//             _text.OutlineColor = Color.Black;
//             _text.OutlineThickness = 1f;
//             _text.DisplayedString = name;
//             _text.Position = Position + new Vector2f(0f, offset);
//             _text.Origin = new Vector2f(0f, 0f);

//             _text.Draw(target, states);
//             offset += TextSpacing;

//             if (!graph.DisplayAverage) continue;
//             float average = MathF.Round(graph.Samples.Sum() / graph.Samples.Count, 2);
//             _text.DisplayedString = $" {average}";
//             _text.CharacterSize = 12;
//             _text.Position = Position + new Vector2f(
//                 Size.X,
//                 Size.Y - average / graph.MaxSampleValue * Size.Y
//             );
//             _text.Origin = new Vector2f(0f, _text.GetLocalBounds().Height / 2f);
//             _text.Draw(target, states);
//         }
//     }
// }