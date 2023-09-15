
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public class SpriteBleedTest : Renderable {


    private static VertexArray vertexArray;
    static SpriteBleedTest() {
        vertexArray = new VertexArray(
            new Layout(VertexAttribPointerType.Float, 2), // position
            new Layout(VertexAttribPointerType.Float, 2)  // uv
        );

        vertexArray.BufferIndices(new int[] {
            0, 1, 2, 1, 2, 3
        }, BufferUsageHint.StaticDraw);
    }

    public override void Render() {

        var textureSize = new Vector2(400f, 400f);
        var textureUV = new Box2(new Vector2(100f, 200f) / textureSize, new Vector2(200f, 300f) / textureSize);

        var spriteSize = new Vector2(100f, 100f);

        var origin = spriteSize * Origin;
        var rotationMatrix = Matrix2.CreateRotation(Rotation);

        var cameraScale = Game.Camera.GetCameraScale(IsUI);

        var t = Game.Time.AsSeconds() / 10f;

        var modelMatrix = Matrix4.Identity;
        modelMatrix *= Matrix4.CreateScale(Scale.X + MathF.Sin(t) * 2.5f, Scale.Y + MathF.Cos(t) * 2.5f, 0f);
        modelMatrix *= Matrix4.CreateRotationZ(-t);
        modelMatrix *= Matrix4.CreateTranslation(MathF.Cos(t) * 300f, MathF.Sin(t) * 300f, 0f);

        var projectionMatrix = Matrix4.CreateOrthographicOffCenter(
            Game.WindowSize.X * -cameraScale / 2f,
            Game.WindowSize.X * cameraScale / 2f,
            Game.WindowSize.Y * -cameraScale / 2f,
            Game.WindowSize.Y * cameraScale / 2f,
             -1f, 1f);





        float[] vertices = new float[32];

        var bl = Vector2.Zero - origin; // model space
        vertices[0] = bl.X;
        vertices[1] = bl.Y;
        vertices[2] = textureUV.Min.X;
        vertices[3] = textureUV.Min.Y;

        // bottom right
        var br = Vector2.UnitX * spriteSize - origin; // model space
        vertices[4] = br.X;
        vertices[5] = br.Y;
        vertices[6] = textureUV.Max.X;
        vertices[7] = textureUV.Min.Y;

        // top left
        var tl = Vector2.UnitY * spriteSize - origin; // model space
        vertices[8] = tl.X;
        vertices[9] = tl.Y;
        vertices[10] = textureUV.Min.X;
        vertices[11] = textureUV.Max.Y;

        // top right
        var tr = Vector2.One * spriteSize - origin; // model space
        vertices[12] = tr.X;
        vertices[13] = tr.Y;
        vertices[14] = textureUV.Max.X;
        vertices[15] = textureUV.Max.Y;


        vertexArray.BufferVertexData(vertices, BufferUsageHint.DynamicDraw);

        vertexArray.Bind();

        Game.Renderer.ShaderLibrary.UseShader("spriteb");

        Game.Renderer.ShaderLibrary.Uniform("modelMatrix", modelMatrix);
        Game.Renderer.ShaderLibrary.Uniform("projectionMatrix", projectionMatrix);

        Game.Renderer.ShaderLibrary.Uniform("inColor", Color4.White);
        Game.Renderer.ShaderLibrary.Uniform("useColorSwapping", false);

        Game.Renderer.TextureLibrary.UseTexture("spritebleedtest", TextureUnit.Texture0);

        GL.DrawElements(PrimitiveType.Triangles, vertexArray.IndexCount, DrawElementsType.UnsignedInt, 0);

    }
}