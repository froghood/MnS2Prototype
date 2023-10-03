using System.Runtime.CompilerServices;

namespace Touhou;

public static class Log {
    public static void Info(object message, [CallerFilePath] string caller = "") {
        var name = Path.GetFileNameWithoutExtension(caller);

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("[I] ");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{name}] ");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(message);
    }

    public static void Warn(object message, [CallerFilePath] string caller = "") {
        var name = Path.GetFileNameWithoutExtension(caller);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[W] ");

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write($"[{name}] ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
    }

    public static void Error(object message, [CallerFilePath] string caller = "") {
        var name = Path.GetFileNameWithoutExtension(caller);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("[E] ");

        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write($"[{name}] ");

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
    }

    public static void Raw(object message) => Console.WriteLine(message);
}