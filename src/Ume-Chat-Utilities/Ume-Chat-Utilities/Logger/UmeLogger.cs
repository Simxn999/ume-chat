using Microsoft.Extensions.Logging;

namespace Ume_Chat_Utilities.Logger;

public class UmeLogger : ILogger
{
    private const LogLevel _minLogLevel = LogLevel.Information;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        WriteLog(logLevel, formatter(state, exception));
    }

    private static void WriteLog(LogLevel logLevel, string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var logType = GetLogTitle(logLevel);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"{timestamp} ");

        Console.ForegroundColor = GetLogLevelColor(logLevel);
        Console.Write($"{logType}");

        Console.ResetColor();
        Console.WriteLine($": {message}");
    }

    private static string GetLogTitle(LogLevel logLevel)
    {
        return logLevel == LogLevel.Information ? "Info" : logLevel.ToString();
    }

    private static ConsoleColor GetLogLevelColor(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                return ConsoleColor.Gray;

            case LogLevel.Debug:
                return ConsoleColor.Yellow;

            case LogLevel.Information:
                return ConsoleColor.Blue;

            case LogLevel.Warning:
                return ConsoleColor.DarkYellow;

            case LogLevel.Error:
                return ConsoleColor.Red;

            case LogLevel.Critical:
                return ConsoleColor.DarkRed;

            case LogLevel.None:
            default:
                return Console.ForegroundColor;
        }
    }
}
