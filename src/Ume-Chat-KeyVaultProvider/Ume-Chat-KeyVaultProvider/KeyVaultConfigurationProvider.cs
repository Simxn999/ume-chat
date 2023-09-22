using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace Ume_Chat_KeyVaultProvider;

internal class KeyVaultConfigurationProvider : ConfigurationProvider
{
    private const string _azureKeyVaultURLSettingName = "AZURE_KEY_VAULT_URL";
    private readonly string _azureKeyVaultUrl;

    private readonly IConfiguration _config;
    private readonly IKeyVaultGateway _keyVaultGateway;

    public KeyVaultConfigurationProvider(IConfiguration config, string? azureKeyVaultUrl, IKeyVaultGateway? keyVaultGateway)
    {
        var url = config[_azureKeyVaultURLSettingName] ?? "";

        if (string.IsNullOrEmpty(azureKeyVaultUrl) && string.IsNullOrEmpty(url))
            throw new Exception("Azure Key Vault URL was not set!");

        _config = config;
        _azureKeyVaultUrl = azureKeyVaultUrl ?? url;
        _keyVaultGateway = keyVaultGateway ?? GetDefaultKeyVaultGateway();
    }

    public override void Load()
    {
        var settingsWithKeyVaultRef = FilterKeyVaultReferences();

        foreach (var settingName in settingsWithKeyVaultRef.Keys)
        {
            var keyVaultUrl = settingsWithKeyVaultRef[settingName].KeyVaultURL ?? _azureKeyVaultUrl;
            var secretName = settingsWithKeyVaultRef[settingName].KeyVaultSecretName;

            if (string.IsNullOrEmpty(keyVaultUrl))
                throw new Exception("Azure Key Vault URL was not set!");

            if (string.IsNullOrEmpty(secretName))
                throw new Exception("Secret name was not set!");

            var secret = _keyVaultGateway.GetSecretAsync(secretName, keyVaultUrl).Result.Value;

            Data.Add(settingName, secret.Value);
        }
    }

    private IKeyVaultGateway GetDefaultKeyVaultGateway()
    {
        var credential = new DefaultAzureCredential();
        var sc = new SecretClient(new Uri(_azureKeyVaultUrl), credential);

        return new AzureKeyVaultGateway(sc, credential);
    }

    private IDictionary<string, AzureKeyVaultReference> FilterKeyVaultReferences()
    {
        var references = new Dictionary<string, AzureKeyVaultReference>();
        FilterKeyVaultReferencesRecursive(_config, ref references);

        return references;
    }

    private static void FilterKeyVaultReferencesRecursive(IConfiguration configSection, ref Dictionary<string, AzureKeyVaultReference> references)
    {
        foreach (var childSection in configSection.GetChildren())
        {
            FilterKeyVaultReferencesRecursive(childSection, ref references);

            var value = childSection.Value;
            if (string.IsNullOrEmpty(value) || !value.StartsWith(AzureKeyVaultReference.ConfigValuePrefix))
                continue;

            references.Add(childSection.Path, new AzureKeyVaultReference(value));
        }
    }
}