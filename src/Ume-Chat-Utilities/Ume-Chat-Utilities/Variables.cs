using System.Globalization;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ume_Chat_Utilities;

/// <summary>
///     Retrieves environment variables from app configuration.
/// </summary>
public static class Variables
{
    private static IConfiguration? _configuration;
    private static ILogger? _logger;
    private static ConfigurationClient? _cloudConfigurationClient;

    /// <summary>
    ///     Add variable support to ConfigurationBuilder.
    /// </summary>
    /// <param name="builder">App configuration builder</param>
    /// <param name="cloud">If Azure App Configuration is used</param>
    public static void AddVariables(this IConfigurationBuilder builder, bool cloud = false)
    {
        _configuration = builder.Build();

        if (!cloud)
            return;

        var cloudConnectionString = Get("DATASYNC_APP_CONFIGURATION_CONNECTION_STRING");
        _cloudConfigurationClient = new ConfigurationClient(cloudConnectionString);
    }

    /// <summary>
    ///     Add a logger to the variables instance.
    /// </summary>
    /// <param name="logger">ILogger</param>
    public static void AddLogger(ILogger logger)
    {
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
            ArgumentNullException.ThrowIfNull(_configuration);

            var value = _configuration[key];

            ArgumentNullException.ThrowIfNull(value, $"{nameof(_configuration)}[{key}]");

            return value;
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
            ArgumentNullException.ThrowIfNull(_configuration);

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
            ArgumentNullException.ThrowIfNull(_configuration);

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
            ArgumentNullException.ThrowIfNull(_configuration);

            var output = _configuration.GetSection(key).GetChildren().Select(c => c.Value ?? string.Empty).Where(c => !string.IsNullOrEmpty(c));

            return output ?? throw new Exception("Enumerable not found!");
        }
        catch (Exception e)
        {
            Error(e);
            throw;
        }
    }

    /// <summary>
    ///     Retrieve environment variable as DateTimeOffset from app configuration.
    /// </summary>
    /// <param name="key">Variable name</param>
    /// <returns>DateTimeOffset from app configuration</returns>
    /// <exception cref="Exception">Variable not found exception</exception>
    public static DateTimeOffset GetDateTimeOffset(string key)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_configuration);

            var value = _configuration[key] ?? throw new Exception("Variable not found!");

            return DateTimeOffset.Parse(value);
        }
        catch (Exception e)
        {
            Error(e);
            throw;
        }
    }

    /// <summary>
    ///     Updates a variable in Azure App Configuration.
    /// </summary>
    /// <param name="key">Variable name</param>
    /// <param name="value">Variable value</param>
    /// <exception cref="Exception">Invalid configuration exception</exception>
    public static void Set(string key, string value)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_configuration);
            ArgumentNullException.ThrowIfNull(_cloudConfigurationClient, nameof(_cloudConfigurationClient));

            _cloudConfigurationClient.SetConfigurationSetting(key, value);
            _configuration[key] = value;
        }
        catch (Exception e)
        {
            Error(e);
            throw;
        }
    }

    /// <summary>
    ///     Handles variable errors.
    /// </summary>
    /// <param name="e">Exception</param>
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
