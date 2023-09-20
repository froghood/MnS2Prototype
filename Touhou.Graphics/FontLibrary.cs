using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public class FontLibrary {

    private Dictionary<string, int> fontTextures = new();
    private Dictionary<string, FontData> fontAtlas = new();

    public FontLibrary() {

    }

    public void Load(string imagePath, string dataPath) {
        fontTextures.TryAdd(Path.GetFileNameWithoutExtension(imagePath), Texture.Load(imagePath, false));

        var jsonSource = File.ReadAllText(dataPath);

        var json = JObject.Parse(jsonSource);

        uint width = (uint)json["atlas"]["width"];
        uint height = (uint)json["atlas"]["height"];
        uint sdfSize = (uint)json["atlas"]["size"];

        float lineHeight = (float)json["metrics"]["lineHeight"];
        float ascender = (float)json["metrics"]["ascender"];
        float descender = (float)json["metrics"]["descender"];


        var glyphs = json["glyphs"].Select(e => {

            return new Glyph {
                Unicode = (int)e["unicode"],
                Advance = (float)e["advance"],
                PlaneBounds = e["planeBounds"] != null ? new Box2(
                    new Vector2((float)e["planeBounds"]["left"], (float)e["planeBounds"]["bottom"]),
                    new Vector2((float)e["planeBounds"]["right"], (float)e["planeBounds"]["top"])
                ) : default(Box2),
                TextureBounds = e["atlasBounds"] != null ? new Box2(
                    new Vector2((float)e["atlasBounds"]["left"], (float)e["atlasBounds"]["bottom"]),
                    new Vector2((float)e["atlasBounds"]["right"], (float)e["atlasBounds"]["top"])
                ) : default(Box2)
            };

        }).ToDictionary(glyph => glyph.Unicode);

        fontAtlas.TryAdd(Path.GetFileNameWithoutExtension(dataPath), new FontData {
            TextureWidth = width,
            TextureHeight = height,
            SDFSize = sdfSize,
            LineHeight = lineHeight,
            Ascender = ascender,
            Descender = descender,
            Glyphs = glyphs
        });
    }

    public bool TryUseFont(string name, TextureUnit unit, out FontData data) {
        if (fontAtlas.ContainsKey(name) && fontTextures.ContainsKey(name)) {
            data = fontAtlas[name];

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, fontTextures[name]);

            return true;
        }
        data = null;
        return false;
    }
}