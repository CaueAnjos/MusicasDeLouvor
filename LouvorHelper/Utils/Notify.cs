using Dumpify;

namespace LouvorHelper.Utils;

static class Notify
{
    public static void Info(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    public static void Success(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    public static void Warning(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    public static void Error(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    public static void ObjectDump(Object? obj)
    {
        if (obj is null)
        {
            Notify.Error("Object is null");
            return;
        }
        obj.Dump();
    }
}
