using System.Text.RegularExpressions;

namespace Ume_Chat_KeyVaultProvider;

public class AzureKeyVaultReference
{
    public const string ConfigValuePrefix = "@AzureKeyVault";
    private const string _configValuePattern = ConfigValuePrefix + @"\((.+?)(?:, (.+))?\)";

    public AzureKeyVaultReference(string value)
    {
        if (value.Count(@char => @char == ',') > 1)
            throw new Exception($"Azure Key Vault Reference found more than two parameters! Value = [{value}]");

        var match = Regex.Match(value, _configValuePattern);

        if (!match.Success)
            throw new Exception($"Azure Key Vault Reference could not be parsed! Value = [{value}]");

        var secretName = match.Groups[1].Value;
        var url = match.Groups[2].Success ? match.Groups[2].Value : null;

        if (string.IsNullOrEmpty(secretName))
            throw new Exception("Name of secret could not be parsed!");

        KeyVaultSecretName = secretName;
        KeyVaultURL = url;
    }

    public string KeyVaultSecretName { get; }
    public string? KeyVaultURL { get; }

    public override string ToString()
    {
        return $"{ConfigValuePrefix}({KeyVaultSecretName}{(string.IsNullOrEmpty(KeyVaultURL) ? "" : $", {KeyVaultURL}")})";
    }
}