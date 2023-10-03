using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public class Graph : Renderable {

    public Vector2 Size { get; set; }
    public Color4 Color { get; set; } = Color4.White;

    private List<float> marks = new();
    private Func<float> valueDelegate;
    private int maxMarks;
    private Rectangle background;



    private static VertexArray vertexArray;


    static Graph() {
        vertexArray = new VertexArray(
            new Layout(VertexAttribPointerType.Float, 2)
        );
    }

    public Graph(Func<float> valueDelegate, int maxMarks) {

        this.valueDelegate = valueDelegate;
        this.maxMarks = maxMarks;

        background = new Rectangle();
    }



    public override void Render() {

        // background.Size = Size;

        // background.Origin = Origin;
        // background.Position = Position;
        // background.Rotation = Rotation;
        // background.Scale = Scale;
        // background.IsUI = IsUI;
        // background.UIAlignment = UIAlignment;

        // background.Render();

        var value = valueDelegate.Invoke();

        //Log.Info(value);

        if (valueDelegate != null) marks.Add(value);
        while (marks.Count > maxMarks) marks.RemoveAt(0);

        //Log.Info(maxMarks);
        //Log.Info(marks.Count);


        var largestMark = 0f;
        foreach (var mark in marks) {
            if (mark > largestMark) largestMark = mark;
        }

        //Log.Info(largestMark);


        var rotationMatrix = Matrix2.CreateRotation(Rotation);

        var vertices = new float[marks.Count * 2];
        //var indices = new int[Math.Max(marks.Count - 1, 0) * 2];
        var indices = new int[marks.Count];



        for (int i = 0; i < marks.Count; i++) {
            var vertex = new Vector2(
                ((float)i / maxMarks) * Size.X,
                marks[i] / largestMark * Size.Y
            ) - Size * Origin;

            //Log.Info(vertex);

            vertices[i * 2 + 0] = vertex.X;
            vertices[i * 2 + 1] = vertex.Y;

            // if (i < marks.Count - 1) {
            //     indices[i * 2 + 0] = i;
            //     indices[i * 2 + 1] = i + 1;
            // }

            indices[i] = i;

        }

        vertexArray.BufferVertexData(vertices, BufferUsageHint.StreamDraw);
        vertexArray.BufferIndices(indices, BufferUsageHint.StreamDraw);

        vertexArray.Bind();

        Game.Renderer.ShaderLibrary.UseShader("graph");

        Game.Renderer.ShaderLibrary.Uniform("position", Position);
        Game.Renderer.ShaderLibrary.Uniform("scale", Scale);
        Game.Renderer.ShaderLibrary.Uniform("rotation", rotationMatrix);

        Game.Renderer.ShaderLibrary.Uniform("isUI", IsUI);
        Game.Renderer.ShaderLibrary.Uniform("uiAlignment", Alignment);

        Game.Renderer.ShaderLibrary.Uniform("cameraPosition", Game.Camera.Position);
        Game.Renderer.ShaderLibrary.Uniform("cameraView", Game.Camera.View);
        Game.Renderer.ShaderLibrary.Uniform("windowAspectRatio", Game.AspectRatio);

        Game.Renderer.ShaderLibrary.Uniform("inColor", Color);

        GL.DrawElements(PrimitiveType.LineStrip, vertexArray.IndexCount, DrawElementsType.UnsignedInt, 0);


    }

    private Vector2 TransformModelToNDC(Vector2 vertex, Matrix2 rotationMatrix) {
        // vertex = vertex * rotationMatrix * Scale + Position; // world space

        // vertex = vertex / Game.Camera.GetCameraSize(IsUI) * 2f;
        // vertex += IsUI ? UIAlignment : -Game.Camera.Position;

        return vertex;
    }
}