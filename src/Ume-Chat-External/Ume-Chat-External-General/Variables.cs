using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ume_Chat_External_General;

public static class Variables
{
    private static IConfiguration? _configuration;
    private static ILogger? _logger;

    public static void Initialize(IConfiguration configuration, ILogger? logger = null)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public static string Get(string key)
    {
        try
        {
            if (_configuration is null)
                throw new Exception("Invalid configuration!");

            return _configuration[key] ?? throw new Exception("Variable not found!");
        }
        catch (Exception e)
        {
            if (_logger is null)
            {
                Console.WriteLine("Failed retrieval of variable!");
                Console.WriteLine(e);
                throw;
            }
            
            _logger.LogError(e, "Failed retrieval of variable!");
            throw;
        }
    }
}