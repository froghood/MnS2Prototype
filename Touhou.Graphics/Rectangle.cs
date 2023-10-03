using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public class Rectangle : Renderable {

    public Vector2 Size { get; set; }

    public Color4 FillColor { get; set; } = Color4.White;
    public Color4 StrokeColor { get; set; } = Color4.Black;
    public float StrokeWidth { get; set; }

    private static VertexArray vertexArray;

    public Rectangle() { }
    public Rectangle(Rectangle copy) : base(copy) {
        Size = copy.Size;
        FillColor = copy.FillColor;
        StrokeColor = copy.StrokeColor;
        StrokeWidth = copy.StrokeWidth;
    }

    static Rectangle() {
        vertexArray = new VertexArray(
            new Layout(VertexAttribPointerType.Float, 2) // position
        );

        vertexArray.BufferIndices(new int[] {
            0, 1, 4, // outer
            1, 4, 5,
            0, 2, 4,
            2, 4, 6,
            3, 1, 7,
            1, 7, 5,
            3, 2, 7,
            2, 7, 6,
            4, 5, 6, // inner
            5, 6, 7,
        }, BufferUsageHint.StaticDraw);
    }
    public override void Render() {

        var origin = Size * Origin;
        var rotationMatrix = Matrix2.CreateRotation(Rotation);

        float[] vertices = new float[16];

        // outer bottom left
        var obl = TransformModelToNDC(Vector2.Zero - origin, rotationMatrix); // model space
        vertices[0] = obl.X;
        vertices[1] = obl.Y;

        // outer bottom right
        var obr = TransformModelToNDC(Vector2.UnitX * Size - origin, rotationMatrix); // model space
        vertices[2] = obr.X;
        vertices[3] = obr.Y;

        // outer top left
        var otl = TransformModelToNDC(Vector2.UnitY * Size - origin, rotationMatrix); // model space
        vertices[4] = otl.X;
        vertices[5] = otl.Y;

        // outer top right
        var otr = TransformModelToNDC(Vector2.One * Size - origin, rotationMatrix); // model space
        vertices[6] = otr.X;
        vertices[7] = otr.Y;

        // inner bottom left
        var ibl = TransformModelToNDC(Vector2.Zero + new Vector2(1f, 1f) * StrokeWidth - origin, rotationMatrix); // model space
        vertices[8] = ibl.X;
        vertices[9] = ibl.Y;

        // inner bottom right
        var ibr = TransformModelToNDC(Vector2.UnitX * Size + new Vector2(-1f, 1f) * StrokeWidth - origin, rotationMatrix); // model space
        vertices[10] = ibr.X;
        vertices[11] = ibr.Y;

        // inner top left
        var itl = TransformModelToNDC(Vector2.UnitY * Size + new Vector2(1f, -1f) * StrokeWidth - origin, rotationMatrix); // model space
        vertices[12] = itl.X;
        vertices[13] = itl.Y;

        // inner top right
        var itr = TransformModelToNDC(Vector2.One * Size + new Vector2(-1f, -1f) * StrokeWidth - origin, rotationMatrix); // model space
        vertices[14] = itr.X;
        vertices[15] = itr.Y;



        vertexArray.BufferVertexData(vertices, BufferUsageHint.StreamDraw);

        vertexArray.Bind();

        Game.Renderer.ShaderLibrary.UseShader("rectangle");
        Game.Renderer.ShaderLibrary.Uniform("fillColor", FillColor);
        Game.Renderer.ShaderLibrary.Uniform("strokeColor", StrokeColor);

        GL.DrawElements(PrimitiveType.Triangles, vertexArray.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    private Vector2 TransformModelToNDC(Vector2 vertex, Matrix2 rotationMatrix) {

        vertex = vertex * rotationMatrix * Scale + Position + (!IsUI ? -Game.Camera.Position : Vector2.Zero); // world space

        // if (!IsUI) vertex = vertex * Game.NewCamera.WorldToCameraScale - Game.NewCamera.Position; // camera space
        // vertex = IsUI ? vertex / new Vector2(2160f * Game.AspectRatio, 2160f) * 2f - UIAlignment : vertex / Game.WindowSize * 2f; // clip space

        vertex = vertex / Game.Camera.GetCameraSize(IsUI) * 2f;
        vertex += Alignment;

        return vertex;
    }
}