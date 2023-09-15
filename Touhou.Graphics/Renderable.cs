
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public abstract class Renderable {
    private Rectangle copy;

    protected Renderable() { }

    protected Renderable(Renderable copy) {
        Depth = copy.Depth;
        Origin = copy.Origin;
        Position = copy.Position;
        Rotation = copy.Rotation;
        Scale = copy.Scale;
        IsUI = copy.IsUI;
        Alignment = copy.Alignment;
    }

    public float Depth { get; set; }




    public Vector2 Origin { get; set; }
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;

    public bool IsUI { get; set; } = false;
    public Vector2 Alignment { get; set; }
    public BlendMode BlendMode { get; set; } = BlendMode.Normal;



    public abstract void Render();

    public void Blend() {
        switch (BlendMode) {
            case BlendMode.Normal:
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
                break;
            case BlendMode.Additive:
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                break;
            case BlendMode.Multiply:
                GL.BlendFunc(BlendingFactor.DstColor, BlendingFactor.OneMinusSrcAlpha);
                break;


        }
    }



}