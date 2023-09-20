using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Touhou.Graphics;

public class TextureLibrary {

    private Dictionary<string, int> textures = new();

    public void LoadTexture(string path) {
        textures[Path.GetFileNameWithoutExtension(path)] = Texture.Load(path, true);
    }

    public bool TryGetTexture(string name, out int handle) {
        if (textures.TryGetValue(name, out int value)) {
            handle = value;
            return true;
        }
        handle = 0;
        return false;
    }

    public void UseTexture(string name, TextureUnit unit) {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, textures[name]);
    }
}