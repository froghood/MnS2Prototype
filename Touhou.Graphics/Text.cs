using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public class Text : Renderable {

    public string DisplayedText { get; set; }
    public string Font { get; set; }
    public float Padding { get; set; }

    public Color4 Color { get; set; } = Color4.White;

    public float CharacterSize { get; set; } = 24f;
    public float Boldness { get; set; }

    private static VertexArray vertexArray;

    public Text() { }
    public Text(Text copy) : base(copy) {
        DisplayedText = $"{copy.DisplayedText}";
        Font = $"{copy.Font}";
        Padding = copy.Padding;
        Color = copy.Color;
        CharacterSize = copy.CharacterSize;
        Boldness = copy.Boldness;


    }

    static Text() {
        vertexArray = new VertexArray(
            new Layout(VertexAttribPointerType.Float, 2), // position
            new Layout(VertexAttribPointerType.Float, 2) // uv
        );
    }

    public override void Render() {
        if (Game.Renderer.FontLibrary.TryUseFont("consolas", TextureUnit.Texture0, out var data)) {



            var rotationMatrix = Matrix2.CreateRotation(Rotation);
            var cameraPosition = IsUI ? Vector2.Zero : Game.Camera.Position;

            var modelMatrix = Matrix4.Identity;
            modelMatrix *= Matrix4.CreateScale(Scale.X, Scale.Y, 0f);
            modelMatrix *= Matrix4.CreateRotationZ(Rotation);
            modelMatrix *= Matrix4.CreateTranslation(Position.X, Position.Y, 0f);
            modelMatrix *= Matrix4.CreateTranslation(-cameraPosition.X, -cameraPosition.Y, 0f);

            var cameraScale = Game.Camera.GetCameraScale(IsUI);

            var projectionMatrix = Matrix4.CreateOrthographicOffCenter(
            Game.WindowSize.X * -cameraScale / 2f,
            Game.WindowSize.X * cameraScale / 2f,
            Game.WindowSize.Y * -cameraScale / 2f,
            Game.WindowSize.Y * cameraScale / 2f,
             -1f, 1f);

            float[] vertices = new float[DisplayedText.Length * 4 * 4];
            int[] indices = new int[DisplayedText.Length * 6];

            var glyphs = DisplayedText.Select(e => {
                int unicode = Convert.ToInt32(e);
                return data.Glyphs.ContainsKey(unicode) ? data.Glyphs[unicode] : data.Glyphs.Values.First();
            }).ToArray();

            float textWidth = CalculateTextWidth(glyphs, Padding);
            float textHeight = data.Ascender - data.Descender;
            Vector2 originOffset = new Vector2(textWidth, textHeight) * Origin;

            float advance = 0f;

            float firstGlpyhOffset = glyphs[0].PlaneBounds.Min.X;

            for (int i = 0; i < glyphs.Length; i++) {
                var glyph = glyphs[i];

                int charVerticesOffset = i * 16;
                int charIndicesOffset = i * 6;
                int vertexOffset = i * 4;

                // bottom-left
                var bl = (new Vector2(glyph.PlaneBounds.Min.X + advance - firstGlpyhOffset, glyph.PlaneBounds.Min.Y) - originOffset) * CharacterSize; // local space
                vertices[charVerticesOffset + 0] = bl.X;
                vertices[charVerticesOffset + 1] = bl.Y;
                vertices[charVerticesOffset + 2] = glyph.TextureBounds.Min.X / data.TextureWidth;
                vertices[charVerticesOffset + 3] = glyph.TextureBounds.Min.Y / data.TextureHeight;

                // bottom-right
                var br = (new Vector2(glyph.PlaneBounds.Max.X + advance - firstGlpyhOffset, glyph.PlaneBounds.Min.Y) - originOffset) * CharacterSize;
                vertices[charVerticesOffset + 4] = br.X;
                vertices[charVerticesOffset + 5] = br.Y;
                vertices[charVerticesOffset + 6] = glyph.TextureBounds.Max.X / data.TextureWidth;
                vertices[charVerticesOffset + 7] = glyph.TextureBounds.Min.Y / data.TextureHeight;

                // top-left
                var tl = (new Vector2(glyph.PlaneBounds.Min.X + advance - firstGlpyhOffset, glyph.PlaneBounds.Max.Y) - originOffset) * CharacterSize;
                vertices[charVerticesOffset + 8] = tl.X;
                vertices[charVerticesOffset + 9] = tl.Y;
                vertices[charVerticesOffset + 10] = glyph.TextureBounds.Min.X / data.TextureWidth;
                vertices[charVerticesOffset + 11] = glyph.TextureBounds.Max.Y / data.TextureHeight;

                // top-right
                var tr = (new Vector2(glyph.PlaneBounds.Max.X + advance - firstGlpyhOffset, glyph.PlaneBounds.Max.Y) - originOffset) * CharacterSize;
                vertices[charVerticesOffset + 12] = tr.X;
                vertices[charVerticesOffset + 13] = tr.Y;
                vertices[charVerticesOffset + 14] = glyph.TextureBounds.Max.X / data.TextureWidth;
                vertices[charVerticesOffset + 15] = glyph.TextureBounds.Max.Y / data.TextureHeight;

                indices[charIndicesOffset + 0] = vertexOffset + 0;
                indices[charIndicesOffset + 1] = vertexOffset + 1;
                indices[charIndicesOffset + 2] = vertexOffset + 2;
                indices[charIndicesOffset + 3] = vertexOffset + 1;
                indices[charIndicesOffset + 4] = vertexOffset + 2;
                indices[charIndicesOffset + 5] = vertexOffset + 3;

                advance += glyph.Advance + Padding;
            }



            vertexArray.BufferVertexData(vertices, BufferUsageHint.StreamDraw);
            vertexArray.BufferIndices(indices, BufferUsageHint.StreamDraw);

            vertexArray.Bind();

            Game.Renderer.ShaderLibrary.UseShader("text");

            Game.Renderer.ShaderLibrary.Uniform("cameraPosition", cameraPosition);
            Game.Renderer.ShaderLibrary.Uniform("modelMatrix", modelMatrix);
            Game.Renderer.ShaderLibrary.Uniform("projectionMatrix", projectionMatrix);
            Game.Renderer.ShaderLibrary.Uniform("alignment", Alignment);

            Game.Renderer.ShaderLibrary.Uniform("screenPxRange", data.SDFSize / (CharacterSize / cameraScale) / 80f);
            Game.Renderer.ShaderLibrary.Uniform("textColor", Color);
            Game.Renderer.ShaderLibrary.Uniform("boldness", Boldness);

            GL.DrawElements(PrimitiveType.Triangles, vertexArray.IndexCount, DrawElementsType.UnsignedInt, 0);

        }
    }

    private static float CalculateTextWidth(Glyph[] glpyhs, float padding) {

        float advance = 0f;

        for (int i = 0; i < glpyhs.Length - 1; i++) {
            advance += glpyhs[i].Advance + padding;
        }

        return (advance + glpyhs[glpyhs.Length - 1].PlaneBounds.Max.X) - glpyhs[0].PlaneBounds.Min.X;
    }

    private Vector2 TransformModelToNDC(Vector2 position, Matrix2 rotation) {
        position = position * rotation * Scale + Position; // world space
        position = position / Game.Camera.GetCameraSize(IsUI) * 2f;
        position += IsUI ? Alignment : -Game.Camera.Position;
        return position;
    }
}