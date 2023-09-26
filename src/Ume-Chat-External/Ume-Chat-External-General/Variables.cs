using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ume_Chat_External_General;

/// <summary>
///     Retrieves environment variables from app configuration.
/// </summary>
public static class Variables
{
    private static IConfiguration? _configuration;
    private static ILogger? _logger;

    /// <summary>
    ///     Add variable support to ConfigurationBuilder.
    /// </summary>
    /// <param name="builder">App configuration builder</param>
    /// <param name="logger">ILogger</param>
    public static void AddVariables(this IConfigurationBuilder builder, ILogger? logger = null)
    {
        _configuration = builder.Build();
        _logger = logger;
    }

    /// <summary>
    ///     Retrieve environment variable from app configuration.
    /// </summary>
    /// <param name="key">Variable name</param>
    /// <returns>Variable value as string</returns>
    /// <exception cref="Exception">Variable not found exception</exception>
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
            Error(e);
            throw;
        }
    }

    /// <summary>
    ///     Retrieve environment variable as int from app configuration.
    /// </summary>
    /// <param name="key">Variable name</param>
    /// <returns>Variable value as int</returns>
    /// <exception cref="Exception">Variable not found exception</exception>
    public static int GetInt(string key)
    {
        try
        {
            if (_configuration is null)
                throw new Exception("Invalid configuration!");

            var value = _configuration[key] ?? throw new Exception("Variable not found!");

            return int.Parse(value);
        }
        catch (Exception e)
        {
            Error(e);
            throw;
        }
    }

    /// <summary>
    ///     Retrieve environment variable as float from app configuration.
    /// </summary>
    /// <param name="key">Variable name</param>
    /// <returns>Variable value as float</returns>
    /// <exception cref="Exception">Variable not found exception</exception>
    public static float GetFloat(string key)
    {
        try
        {
            if (_configuration is null)
                throw new Exception("Invalid configuration!");

            var value = _configuration[key] ?? throw new Exception("Variable not found!");

            return float.Parse(value, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            Error(e);
            throw;
        }
    }

    /// <summary>
    ///     Retrieve environment variable enumerable from app configuration.
    /// </summary>
    /// <param name="key">Enumerable name</param>
    /// <returns>Enumerable from app configuration</returns>
    /// <exception cref="Exception">Enumerable not found!</exception>
    public static IEnumerable<string> GetEnumerable(string key)
    {
        try
        {
            if (_configuration is null)
                throw new Exception("Invalid configuration!");

            var output = _configuration.GetSection(key)
                                       .GetChildren()
                                       .Select(c => c.Value ?? string.Empty)
                                       .Where(c => !string.IsNullOrEmpty(c));

            return output ?? throw new Exception("Enumerable not found!");
        }
        catch (Exception e)
        {
            if (_logger is null)
            {
                Console.WriteLine("Failed retrieval of enumerable!");
                Console.WriteLine(e);
                throw;
            }

            _logger.LogError(e, "Failed retrieval of enumerable!");
            throw;
        }
    }

    private static void Error(Exception e)
    {
        if (_logger is null)
        {
            Console.WriteLine("Failed retrieval of variable!");
            Console.WriteLine(e);
            return;
        }

        _logger.LogError(e, "Failed retrieval of variable!");
    }
}
