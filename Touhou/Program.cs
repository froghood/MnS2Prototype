

global using Touhou;
global using OpenTK.Graphics.OpenGL4;

public class Program {
    private static void Main(string[] args) {

        Log.Info("info test");
        Log.Warn("warning test");
        Log.Error("error test");


        try {
            Game.Init(args);
            Game.Run();
        } catch (Exception e) {
            Log.Error(e);
            Console.ReadLine();
        }


    }
}