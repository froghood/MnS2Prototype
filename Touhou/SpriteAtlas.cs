
using SFML.System;
using SFML.Graphics;
using Newtonsoft.Json;

namespace Touhou;

public class SpriteAtlas {

    private Texture spriteSheet;
    private Dictionary<string, IntRect> atlas = new();


    public SpriteAtlas() {

    }

    public void Load(string sheetPath, string dataPath) {
        spriteSheet = new Texture(sheetPath);
        var dataSource = File.ReadAllText(dataPath);
        atlas = JsonConvert.DeserializeObject<Dictionary<string, IntRect>>(dataSource);
    }

    public Sprite GetSprite(string name) {
        if (atlas.ContainsKey(name)) {
            return new Sprite(spriteSheet, atlas[name]);
        }
        return default(Sprite);
    }

}