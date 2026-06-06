using System;

namespace TEST.Services;

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[LOG]: {message}");
    }
}