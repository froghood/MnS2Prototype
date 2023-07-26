using SFML.Graphics;

namespace Touhou;

public class ShaderLibrary {

    private Texture spriteSheet;
    private Dictionary<string, Shader> atlas = new();

    public void Load(string shadersDirectory) {
        foreach (var path in Directory.GetFiles(shadersDirectory)) {
            var name = Path.GetFileNameWithoutExtension(path);

            Console.ResetColor();
            System.Console.WriteLine($"loading shader: {name}");

            using var reader = new StreamReader(path, System.Text.Encoding.UTF8);

            Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine(reader.ReadToEnd());


            atlas.Add(name, new Shader(null, null, reader.BaseStream));
        }

    }

    public bool GetShader(string name, out Shader shader) {
        if (atlas.ContainsKey(name)) {
            shader = atlas[name];
            return true;
        }
        shader = default(Shader);
        return false;
    }

}