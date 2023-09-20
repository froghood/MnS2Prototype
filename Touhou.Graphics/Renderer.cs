
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Touhou.Graphics;

public class Renderer {

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
    public TextureLibrary TextureLibrary { get => textureLibrary; }
    public ShaderLibrary ShaderLibrary { get => shaderLibrary; }

    public FontLibrary FontLibrary { get => fontLibrary; }

    private IGLFWGraphicsContext context;


    private PriorityQueue<Renderable, float> renderableQueue = new();

    private Queue<Renderable>[] renderableLayers;

    //private Queue<(RenderableType Type, string Shader, List<Renderable> Renderables)> renderableGroups = new();


    private TextureLibrary textureLibrary;
    private TextureAtlas textureAtlas;
    private ShaderLibrary shaderLibrary;
    private FontLibrary fontLibrary;

    public Renderer(IGLFWGraphicsContext context) {

        this.context = context;

        textureAtlas = new TextureAtlas();
        textureAtlas.Load("./assets/sprites/data.json");

        textureLibrary = new TextureLibrary();
        textureLibrary.LoadTexture("./assets/sprites/sprites.png");
        textureLibrary.LoadTexture("./assets/sprites/spritebleedtest.png");

        shaderLibrary = new ShaderLibrary();
        shaderLibrary.LoadShader("./assets/shaders/sprite.vert", ShaderType.VertexShader);
        shaderLibrary.LoadShader("./assets/shaders/sprite.frag", ShaderType.FragmentShader);

        shaderLibrary.LoadShader("./assets/shaders/text.vert", ShaderType.VertexShader);
        shaderLibrary.LoadShader("./assets/shaders/text.frag", ShaderType.FragmentShader);

        shaderLibrary.LoadShader("./assets/shaders/rectangle.vert", ShaderType.VertexShader);
        shaderLibrary.LoadShader("./assets/shaders/rectangle.frag", ShaderType.FragmentShader);

        shaderLibrary.LoadShader("./assets/shaders/graph.vert", ShaderType.VertexShader);
        shaderLibrary.LoadShader("./assets/shaders/graph.frag", ShaderType.FragmentShader);

        shaderLibrary.LoadShader("./assets/shaders/circle.vert", ShaderType.VertexShader);
        shaderLibrary.LoadShader("./assets/shaders/circle.frag", ShaderType.FragmentShader);

        shaderLibrary.LoadShader("./assets/shaders/spriteb.vert", ShaderType.VertexShader);
        shaderLibrary.LoadShader("./assets/shaders/spriteb.frag", ShaderType.FragmentShader);


        fontLibrary = new FontLibrary();
        fontLibrary.Load("./assets/fonts/consolas.png", "./assets/fonts/consolas.json");

        renderableLayers = Enum.GetNames<Layers>().Select(_ => new Queue<Renderable>()).ToArray();

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

    public void Queue(Renderable renderable, Layers layer) {

        renderableLayers[(int)layer].Enqueue(renderable);

    }

}