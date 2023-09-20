
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Touhou.Graphics;
public class VertexArray {

    public int IndexCount { get => indexCount; }

    private int vao;
    private int vbo;
    private int ibo;

    private int indexCount;

    private List<(VertexAttribPointerType Type, int NumberOfComponents)> attributes = new();

    public VertexArray(params Layout[] attributes) {
        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        ibo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);

        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);



        int stride = attributes.Sum(e => e.GetSize());

        int offset = 0;
        for (int i = 0; i < attributes.Length; i++) {
            var attribute = attributes[i];
            GL.VertexAttribPointer(i, attribute.NumberOfComponents, attribute.Type, false, stride, offset);
            GL.EnableVertexAttribArray(i);

            offset += attribute.GetSize();
        }
    }

    public void BufferIndices(int[] data, BufferUsageHint usage) {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(int), data, usage);

        indexCount = data.Length;
    }

    public void BufferVertexData(float[] data, BufferUsageHint usage) {

        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, usage);
    }

    public void Bind() => GL.BindVertexArray(vao);

}