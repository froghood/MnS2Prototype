using OpenTK.Graphics.OpenGL4;

namespace Touhou.Graphics;

public struct Layout {

    public Layout(VertexAttribPointerType type, int numberOfComponents) {
        Type = type;
        NumberOfComponents = numberOfComponents;
    }
    public VertexAttribPointerType Type { get; }
    public int NumberOfComponents { get; }

    public int GetTypeSize() {
        return Type switch {
            VertexAttribPointerType.Byte => sizeof(byte),
            VertexAttribPointerType.Short => sizeof(short),
            VertexAttribPointerType.Int => sizeof(int),
            VertexAttribPointerType.Float => sizeof(float),
            VertexAttribPointerType.Double => sizeof(double),
            _ => throw new Exception("Vertex attribute pointer type size not implemented.")
        };
    }

    public int GetSize() {
        return GetTypeSize() * NumberOfComponents;
    }
}