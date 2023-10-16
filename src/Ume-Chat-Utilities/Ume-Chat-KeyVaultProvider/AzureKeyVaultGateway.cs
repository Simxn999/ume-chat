using Azure;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;

namespace Ume_Chat_KeyVaultProvider;

internal class AzureKeyVaultGateway(SecretClient secretClient, TokenCredential credential) : IKeyVaultGateway
{
    public async Task<Response<KeyVaultSecret>> GetSecretAsync(string secretName, string keyVaultUrl)
    {
        secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
        var secret = await secretClient.GetSecretAsync(secretName);

        return secret;
    }
}
