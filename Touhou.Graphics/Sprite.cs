
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public class Sprite : Renderable {

    public string SpriteName { get; set; }
    public Color4 Color { get; set; } = Color4.White;

    public Vector2 UVPaddingOffset { get; set; } = Vector2.Zero;

    public bool UseColorSwapping { get; set; } = false;

    public Sprite(string spriteName) {
        SpriteName = spriteName;
    }

    public Sprite(Sprite copy) : base(copy) {
        SpriteName = $"{copy.SpriteName}";
        Color = copy.Color;
        UVPaddingOffset = copy.UVPaddingOffset;
        UseColorSwapping = copy.UseColorSwapping;
    }



    private static VertexArray vertexArray;

    static Sprite() {
        vertexArray = new VertexArray(
            new Layout(VertexAttribPointerType.Float, 2), // position
            new Layout(VertexAttribPointerType.Float, 2)  // uv
        );

        vertexArray.BufferIndices(new int[] { 0, 1, 2, 1, 2, 3 }, BufferUsageHint.StreamDraw);
    }

    public override void Render() {

        var textureUV = Game.Renderer.TextureAtlas.GetUVTuple(SpriteName, UVPaddingOffset);
        var textureSize = Game.Renderer.TextureAtlas.GetSize(SpriteName);

        // model + projection matrix
        var cameraPosition = IsUI ? Vector2.Zero : Game.Camera.Position;
        var cameraScale = Game.Camera.GetCameraScale(IsUI) / 2f;

        var modelProjectionMatrix =
              Matrix4.CreateTranslation(-Origin.X, -Origin.Y, 0f)
            * Matrix4.CreateScale(textureSize.X * Scale.X, textureSize.Y * Scale.Y, 0f)
            * Matrix4.CreateRotationZ(Rotation)
            * Matrix4.CreateTranslation(Position.X - cameraPosition.X, Position.Y - cameraPosition.Y, 0f)
            * Matrix4.CreateOrthographicOffCenter(
                Game.WindowSize.X * -cameraScale,
                Game.WindowSize.X * cameraScale,
                Game.WindowSize.Y * -cameraScale,
                Game.WindowSize.Y * cameraScale,
                -1f, 1f
            );

        float[] vertices = new float[] {
            0f, 0f, textureUV.BottomLeft.X,  textureUV.BottomLeft.Y,
            1f, 0f, textureUV.BottomRight.X, textureUV.BottomRight.Y,
            0f, 1f, textureUV.TopLeft.X,     textureUV.TopLeft.Y,
            1f, 1f, textureUV.TopRight.X,    textureUV.TopRight.Y,
        };

        vertexArray.BufferVertexData(vertices, BufferUsageHint.DynamicDraw);
        vertexArray.Bind();

        Game.Renderer.ShaderLibrary.UseShader("spriteb");

        Game.Renderer.ShaderLibrary.Uniform("modelProjectionMatrix", modelProjectionMatrix);
        Game.Renderer.ShaderLibrary.Uniform("alignment", Alignment);
        Game.Renderer.ShaderLibrary.Uniform("inColor", Color);
        Game.Renderer.ShaderLibrary.Uniform("useColorSwapping", UseColorSwapping);

        Game.Renderer.TextureLibrary.UseTexture("sprites", TextureUnit.Texture0);

        GL.DrawElements(PrimitiveType.Triangles, vertexArray.IndexCount, DrawElementsType.UnsignedInt, 0);
    }
}