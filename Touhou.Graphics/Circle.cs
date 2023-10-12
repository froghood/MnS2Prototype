using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public class Circle : Renderable {

    public float Radius { get; set; }
    public int SideCount { get; set; } = 32;
    public float StrokeWidth { get; set; } = 0f;

    public Color4 StrokeColor { get; set; } = Color4.Transparent;
    public Color4 FillColor { get; set; } = Color4.White;

    private static VertexArray vertexArray;

    static Circle() {
        vertexArray = new VertexArray(
            new Layout(VertexAttribPointerType.Float, 2) // position
        );
    }
    public override void Render() {
        if (SideCount < 3) return;

        var origin = new Vector2(Radius) - Origin * Radius * 2f;
        var rotationMatrix = Matrix2.CreateRotation(Rotation);

        var vertices = new float[(SideCount * 2 + 1) * 2];
        var indices = new int[SideCount * 9]; // 3 tris per side, 3 indices per tri

        // center
        vertices[0] = origin.X;
        vertices[1] = origin.Y;

        for (int i = 0; i < SideCount; i++) {
            float angle = MathF.Tau / SideCount * i;

            var innerVertex = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Radius - StrokeWidth) + origin;
            var outerVertex = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Radius + origin;

            vertices[2 + i * 2 + 0] = innerVertex.X;
            vertices[2 + i * 2 + 1] = innerVertex.Y;
            vertices[2 + SideCount * 2 + i * 2 + 0] = outerVertex.X;
            vertices[2 + SideCount * 2 + i * 2 + 1] = outerVertex.Y;

            // center tris
            indices[i * 3 + 0] = 0;
            indices[i * 3 + 1] = i + 1;
            indices[i * 3 + 2] = (i + 1) % SideCount + 1;

            //stroke tris inner
            indices[SideCount * 3 + i * 3 + 0] = i + 1;
            indices[SideCount * 3 + i * 3 + 1] = (i + 1) % SideCount + 1;
            indices[SideCount * 3 + i * 3 + 2] = i + 1 + SideCount;

            // stroke tris outer
            indices[SideCount * 6 + i * 3 + 0] = (i + 1) % SideCount + 1;
            indices[SideCount * 6 + i * 3 + 1] = i + 1 + SideCount;
            indices[SideCount * 6 + i * 3 + 2] = (i + 1) % SideCount + 1 + SideCount;
        }

        vertexArray.BufferVertexData(vertices, BufferUsageHint.StreamDraw);
        vertexArray.BufferIndices(indices, BufferUsageHint.StreamDraw);

        vertexArray.Bind();

        Game.Renderer.ShaderLibrary.UseShader("circle");

        Game.Renderer.ShaderLibrary.Uniform("position", Position - (IsUI ? Vector2.Zero : Game.Camera.Position));
        Game.Renderer.ShaderLibrary.Uniform("scale", Scale);
        Game.Renderer.ShaderLibrary.Uniform("rotation", rotationMatrix);

        Game.Renderer.ShaderLibrary.Uniform("isUI", IsUI);
        Game.Renderer.ShaderLibrary.Uniform("uiAlignment", Alignment);

        Game.Renderer.ShaderLibrary.Uniform("cameraPosition", Game.Camera.Position);
        Game.Renderer.ShaderLibrary.Uniform("cameraView", Game.Camera.View);
        Game.Renderer.ShaderLibrary.Uniform("windowAspectRatio", Game.AspectRatio);

        Game.Renderer.ShaderLibrary.Uniform("strokeColor", StrokeColor);
        Game.Renderer.ShaderLibrary.Uniform("fillColor", FillColor);

        GL.DrawElements(PrimitiveType.Triangles, vertexArray.IndexCount, DrawElementsType.UnsignedInt, 0);




    }
}