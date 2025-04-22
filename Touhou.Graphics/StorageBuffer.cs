using System.Runtime.InteropServices;
using FrogLib;
using OpenTK.Graphics.OpenGL4;

namespace OsuEditor;

public class StorageBuffer {



    public int End { get; private set; }
    public int Handle { get; }



    public StorageBuffer() {
        GL.CreateBuffers(1, out int handle);
        Handle = handle;
    }

    public StorageBuffer(int size, BufferUsageHint hint = BufferUsageHint.StreamDraw) : this() {
        Allocate(size, hint);
    }



    public void Use(int index) => GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, index, Handle);

    public void Allocate(int size, BufferUsageHint hint = BufferUsageHint.StreamDraw) {
        GL.NamedBufferData(Handle, size, 0, hint);
        End = 0;
    }

    public void BufferData<T>(T[] data, BufferUsageHint hint = BufferUsageHint.StreamDraw) where T : struct {
        var size = Marshal.SizeOf<T>() * data.Length;
        GL.NamedBufferData(Handle, size, data, hint);
        End = size;
    }

    public void BufferData<T>(T data, BufferUsageHint hint = BufferUsageHint.StreamDraw) where T : struct {
        var size = Marshal.SizeOf<T>();
        GL.NamedBufferData(Handle, size, ref data, hint);
        End = size;
    }

    public void BufferSubData<T>(T[] data, int offset = 0) where T : struct {
        var size = Marshal.SizeOf<T>() * data.Length;
        GL.NamedBufferSubData(Handle, offset, size, data);
        End = Math.Max(End, offset + size);
    }

    public void BufferSubData<T>(T data, int offset = 0) where T : struct {
        var size = Marshal.SizeOf<T>();
        GL.NamedBufferSubData(Handle, offset, size, ref data);
        End = Math.Max(End, offset + size);
    }

    public void AppendData<T>(T[] data) where T : struct {
        var size = Marshal.SizeOf<T>() * data.Length;
        GL.NamedBufferSubData(Handle, End, size, data);
        End += size;
    }

    public void AppendData<T>(T data) where T : struct {
        var size = Marshal.SizeOf<T>();
        GL.NamedBufferSubData(Handle, End, size, ref data);
        End += size;
    }

    public void GetData<T>(T[] data, int offset = 0) where T : struct {
        var size = Marshal.SizeOf<T>() * data.Length;
        GL.GetNamedBufferSubData(Handle, offset, size, data);
    }

    public T GetData<T>(int offset = 0) where T : struct {
        T data = default;
        var size = Marshal.SizeOf<T>();
        GL.GetNamedBufferSubData<T>(Handle, offset, size, ref data);
        return data;
    }

    public void CopyData(StorageBuffer other, int size, int sourceOffset = 0, int destOffset = 0) {
        GL.CopyNamedBufferSubData(Handle, other.Handle, sourceOffset, destOffset, size);
    }

    // public void ShiftData(int offset, int size, int newOffset) {
    //     GL.CopyNamedBufferSubData(Handle, Handle, offset, newOffset, size);
    //     if (offset + size >= End) {
    //         End = Math.Max(End + (newOffset - offset), offset);
    //     } else {
    //         End = Math.Max(End, newOffset + size);
    //     }
    // }

    public void Clear() => End = 0;

}