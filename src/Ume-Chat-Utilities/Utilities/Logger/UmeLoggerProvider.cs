using Microsoft.Extensions.Logging;

namespace Utilities.Logger;

public class UmeLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new UmeLogger();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
