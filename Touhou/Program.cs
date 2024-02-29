

global using Touhou;
global using OpenTK.Graphics.OpenGL4;

public class Program {
    private static void Main(string[] args) {

        try {
            Game.Init(args);
            Game.Run();
        } catch (Exception e) {
            Log.Error(e);
            Console.ReadLine();
        }

    }
}