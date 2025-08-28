
using FrogLib;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Touhou.Graphics;

public class Renderer : GameSystem {

    public Color4 ClearColor {
        get {
            var data = new float[4];
            GL.GetFloat(GetPName.ColorClearValue, data);

            return new Color4(data[0], data[1], data[2], data[3]);
        }
        set {
            GL.ClearColor(value);
        }
    }

    public TextureAtlas TextureAtlas { get => textureAtlas; }
    public TTextureLibrary TextureLibrary { get => textureLibrary; }
    public TShaderLibrary ShaderLibrary { get => TShaderLibrary; }

    public FontLibrary FontLibrary { get => fontLibrary; }

    private IGLFWGraphicsContext context;


    private PriorityQueue<Renderable, float> renderableQueue = new();

    private Queue<Renderable>[] renderableLayers;

    //private Queue<(RenderableType Type, string Shader, List<Renderable> Renderables)> renderableGroups = new();


    private TTextureLibrary textureLibrary;
    private TextureAtlas textureAtlas;
    private TShaderLibrary TShaderLibrary;
    private FontLibrary fontLibrary;

    public Renderer(IGLFWGraphicsContext context) {

        this.context = context;

        textureAtlas = new TextureAtlas();
        textureAtlas.Load("./assets/sprites/sprites.json");

        textureLibrary = new TTextureLibrary();
        textureLibrary.LoadTexture("./assets/sprites/sprites.png");
        textureLibrary.LoadTexture("./assets/sprites/spritebleedtest.png");

        TShaderLibrary = new TShaderLibrary();
        TShaderLibrary.LoadShader("./assets/shaders/sprite.vert", ShaderType.VertexShader);
        TShaderLibrary.LoadShader("./assets/shaders/sprite.frag", ShaderType.FragmentShader);

        TShaderLibrary.LoadShader("./assets/shaders/text.vert", ShaderType.VertexShader);
        TShaderLibrary.LoadShader("./assets/shaders/text.frag", ShaderType.FragmentShader);

        TShaderLibrary.LoadShader("./assets/shaders/rectangle.vert", ShaderType.VertexShader);
        TShaderLibrary.LoadShader("./assets/shaders/rectangle.frag", ShaderType.FragmentShader);

        TShaderLibrary.LoadShader("./assets/shaders/graph.vert", ShaderType.VertexShader);
        TShaderLibrary.LoadShader("./assets/shaders/graph.frag", ShaderType.FragmentShader);

        TShaderLibrary.LoadShader("./assets/shaders/circle.vert", ShaderType.VertexShader);
        TShaderLibrary.LoadShader("./assets/shaders/circle.frag", ShaderType.FragmentShader);

        TShaderLibrary.LoadShader("./assets/shaders/spriteb.vert", ShaderType.VertexShader);
        TShaderLibrary.LoadShader("./assets/shaders/spriteb.frag", ShaderType.FragmentShader);


        fontLibrary = new FontLibrary();
        fontLibrary.Load("./assets/fonts/consolas.png", "./assets/fonts/consolas.json");

        renderableLayers = Enum.GetNames<Layer>().Select(_ => new Queue<Renderable>()).ToArray();

        Log.Info($"Max texture size: {GL.GetInteger(GetPName.MaxTextureSize)}");

        GL.Enable(EnableCap.Blend);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        //GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);

        //GL.Enable(EnableCap.Multisample);

        GL.DepthMask(false);

        GL.ProvokingVertex(ProvokingVertexMode.FirstVertexConvention);

        var bindingsContext = new GLFWBindingsContext();
        GL.LoadBindings(bindingsContext);
    }

    public void Render() {

        GL.Clear(ClearBufferMask.ColorBufferBit);

        foreach (var renderableLayer in renderableLayers) {
            while (renderableLayer.Count > 0) {

                var renderable = renderableLayer.Dequeue();
                renderable.Blend();
                renderable.Render();

            }
        }

        context.SwapBuffers();
    }

    public void Queue(Renderable renderable, Layer layer) {

        renderableLayers[(int)layer].Enqueue(renderable);

    }

}