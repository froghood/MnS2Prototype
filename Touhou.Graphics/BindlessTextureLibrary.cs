using FrogLib;
using OpenTK.Graphics.OpenGL4;

namespace OsuEditor;

public class BindlessTextureLibrary : GameSystem {

    private StorageBuffer textureBuffer = new();
    private List<BindlessTexture> textures = new();
    private Dictionary<string, int> indices = new();


    public void Add(string name, BindlessTexture texture) {

        if (indices.ContainsKey(name)) throw new Exception($"texture \"{name}\" already exists");

        indices.Add(name, textures.Count);
        textures.Add(texture);

        Touhou.Log.Info($"texture \"{name}\" added");
    }

    public BindlessTexture GetTexture(string name) {
        if (!indices.TryGetValue(name, out var index)) return BindlessTexture.Empty;
        return textures[index];
    }
    public int GetIndex(string name) {
        if (!indices.TryGetValue(name, out var index)) return -1;
        return index;
    }

    public BindlessTextureData[] GetBindlessTextureData() {
        var data = new BindlessTextureData[textures.Count];
        for (int i = 0; i < textures.Count; i++) {
            var texture = textures[i];
            data[i] = new BindlessTextureData(texture.Handle);
        }
        return data;
    }

    public void BufferTextures() {
        textureBuffer.BufferData(GetBindlessTextureData(), BufferUsageHint.StaticDraw);
    }

    public unsafe BindlessTextureData[] GetData() {
        var data = new BindlessTextureData[textures.Count];
        GL.GetNamedBufferSubData(textureBuffer.Handle, 0, sizeof(BindlessTextureData) * data.Length, data);
        return data;
    }

    public void UseBuffer(int index) => textureBuffer.Use(index);
}