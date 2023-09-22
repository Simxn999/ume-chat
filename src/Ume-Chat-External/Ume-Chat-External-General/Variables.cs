﻿using Microsoft.Extensions.Configuration;
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
    ///     Initialize class with app configuration.
    /// </summary>
    /// <param name="configuration">App configuration</param>
    /// <param name="logger">ILogger</param>
    public static void Initialize(IConfiguration configuration, ILogger? logger = null)
    {
        _configuration = configuration;
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