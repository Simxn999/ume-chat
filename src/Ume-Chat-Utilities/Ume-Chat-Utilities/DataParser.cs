using System.Text.Json;

namespace Ume_Chat_Utilities;

/// <summary>
///     Parse files into objects.
/// </summary>
public static class DataParser
{
    /// <summary>
    ///     Reads a JSON file and parses it to the specified type.
    /// </summary>
    /// <param name="filePath">Path to JSON file</param>
    /// <typeparam name="T">Type to convert JSON to</typeparam>
    /// <returns>Object of specified type containing data from JSON file</returns>
    public static T LoadJson<T>(string filePath)
    {
        var path = Path.Combine(AppContext.BaseDirectory, filePath);
        using var reader = new StreamReader(path);
        var json = reader.ReadToEnd();
        var output = JsonSerializer.Deserialize<T>(json);

        ArgumentNullException.ThrowIfNull(output);

        return output;
    }
}
