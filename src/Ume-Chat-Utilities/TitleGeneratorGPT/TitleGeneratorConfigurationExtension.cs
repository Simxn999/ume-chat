using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace TitleGeneratorGPT;

/// <summary>
///     IConfigurationBuilder extension for adding Title Generator configurations.
/// </summary>
public static class TitleGeneratorConfigurationExtension
{
    /// <summary>
    ///     Add Title Generator configurations to IConfigurationBuilder.
    /// </summary>
    /// <param name="builder">IConfigurationBuilder to add configuration on</param>
    /// <param name="isDevelopment">If environment is development or not</param>
    public static void AddTitleGenerator(this IConfigurationBuilder builder, bool isDevelopment)
    {
        var assembly = Assembly.GetAssembly(typeof(TitleGenerator));
        var assemblyName = assembly?.GetName().Name;
        ArgumentNullException.ThrowIfNull(assembly);

        var stream = assembly.GetManifestResourceStream($"{assemblyName}.appsettings.json");
        ArgumentNullException.ThrowIfNull(stream);

        builder.AddJsonStream(stream);

        if (!isDevelopment)
            return;

        stream = assembly.GetManifestResourceStream($"{assemblyName}.appsettings.Development.json");
        ArgumentNullException.ThrowIfNull(stream);

        builder.AddJsonStream(stream);
    }
}
