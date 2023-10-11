

using Newtonsoft.Json;
using OpenTK.Mathematics;
using System.Text.Json.Nodes;
using Touhou.Graphics;

namespace Touhou;

public class TextureAtlas {

    [JsonProperty]
    private int width;

    [JsonProperty]
    private int height;

    [JsonProperty]
    private Dictionary<string, SubTexture> sprites = new();


    public void Load(string dataPath) {
        var data = File.ReadAllText(dataPath);
        JsonConvert.PopulateObject(data, this);

        // foreach (var item in sprites) {
        //     Log.Info($"{item.Key}, {item.Value.Left}");
        // }

        Log.Info("Loaded textures");
    }

    public Box2 GetUV(string name) {



        var size = new Vector2(width, height);

        //Log.Info(size);

        if (sprites.TryGetValue(name, out var bounds)) {

            //Log.Info($"boudns: {bounds}");

            var bottomLeft = new Vector2(bounds.Left, size.Y - (bounds.Top + bounds.Height));

            if (name == "reimu") {
                //Log.Info($"{new Vector2(bounds.Left, size.Y - (bounds.Top + bounds.Height))} | {new Vector2(bounds.Width, bounds.Height)}");
            }

            return new Box2(new Vector2(bounds.Left + 0.5f, size.Y - bounds.Top - 0.5f) / size, new Vector2(bounds.Left + bounds.Width - 0.5f, size.Y - (bounds.Top + bounds.Height) + 0.5f) / size);



        } else {
            return default(Box2);
        }
    }

    internal Vector2i GetSize(string name) {
        if (sprites.TryGetValue(name, out var bounds)) {
            return new Vector2i(bounds.Width, bounds.Height);
        } else {
            return default(Vector2i);
        }
    }

    // public string GetTextureName(string name) {

    // }
}

public struct SubTexture {

    // public SubTexture(int left, int top, int width, int height) {
    //     Left = left;
    //     Top = top;
    //     Width = width;
    //     Height = height;
    // }


    public int Left { get; init; }


    public int Top { get; init; }


    public int Width { get; init; }


    public int Height { get; init; }

}