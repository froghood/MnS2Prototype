
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public class Sprite : Renderable {

    public string SpriteName { get; set; }
    public Color4 Color { get; set; } = Color4.White;

    public bool UseColorSwapping { get; set; } = false;

    public Sprite(string spriteName) {
        SpriteName = spriteName;
    }

    public Sprite(Sprite copy) : base(copy) {
        SpriteName = copy.SpriteName;
        Color = copy.Color;
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



        var textureUV = Game.Renderer.TextureAtlas.GetUV(SpriteName);
        var textureSize = Game.Renderer.TextureAtlas.GetSize(SpriteName);

        var origin = textureSize * Origin;

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

        float[] vertices = new float[32];

        // bottom left
        var bl = Vector2.Zero - origin; // model space
        vertices[0] = bl.X;
        vertices[1] = bl.Y;
        vertices[2] = textureUV.Min.X;
        vertices[3] = textureUV.Min.Y;

        // bottom right
        var br = Vector2.UnitX * textureSize - origin; // model space
        vertices[4] = br.X;
        vertices[5] = br.Y;
        vertices[6] = textureUV.Max.X;
        vertices[7] = textureUV.Min.Y;

        // top left
        var tl = Vector2.UnitY * textureSize - origin; // model space
        vertices[8] = tl.X;
        vertices[9] = tl.Y;
        vertices[10] = textureUV.Min.X;
        vertices[11] = textureUV.Max.Y;

        // top right
        var tr = Vector2.One * textureSize - origin; // model space
        vertices[12] = tr.X;
        vertices[13] = tr.Y;
        vertices[14] = textureUV.Max.X;
        vertices[15] = textureUV.Max.Y;

        vertexArray.BufferVertexData(vertices, BufferUsageHint.DynamicDraw);

        vertexArray.Bind();

        Game.Renderer.ShaderLibrary.UseShader("spriteb");

        Game.Renderer.ShaderLibrary.Uniform("cameraPosition", cameraPosition);
        Game.Renderer.ShaderLibrary.Uniform("modelMatrix", modelMatrix);
        Game.Renderer.ShaderLibrary.Uniform("projectionMatrix", projectionMatrix);
        Game.Renderer.ShaderLibrary.Uniform("alignment", Alignment);

        Game.Renderer.ShaderLibrary.Uniform("inColor", Color);
        Game.Renderer.ShaderLibrary.Uniform("useColorSwapping", UseColorSwapping);

        Game.Renderer.TextureLibrary.UseTexture("sprites", TextureUnit.Texture0);



        GL.DrawElements(PrimitiveType.Triangles, vertexArray.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    private Vector2 TransformModelToNDC(Vector2 position, Matrix2 rotation) {
        //if (!IsUI) System.Console.WriteLine(Game.Camera.GetCameraScale(IsUI));

        position = position * rotation * Scale + Position; // world space
        position = position / ((Vector2)Game.WindowSize * Game.Camera.GetCameraScale(IsUI)) * 2f;
        position += IsUI ? Alignment : -Game.Camera.Position;
        return position;
    }
}