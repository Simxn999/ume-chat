using Microsoft.Extensions.Logging;

namespace Ume_Chat_Utilities.Logger;

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
