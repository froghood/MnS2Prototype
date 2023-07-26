
using SFML.System;
using SFML.Graphics;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace Touhou;

public class SpriteAtlas {

    private Texture spriteSheet;
    private Dictionary<string, IntRect> atlas = new();


    public SpriteAtlas() {

    }

    public void Load(string sheetPath, string dataPath) {
        spriteSheet = new Texture(sheetPath);
        var json = File.ReadAllText(dataPath);
        var sprites = JsonObject.Parse(json)["Sprites"];
        atlas = JsonConvert.DeserializeObject<Dictionary<string, IntRect>>(sprites.ToJsonString());
    }

    public Sprite GetSprite(string name) {
        if (atlas.ContainsKey(name)) {
            return new Sprite(spriteSheet, atlas[name]);
        }
        return default(Sprite);
    }

}