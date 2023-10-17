using Azure;
using Azure.Security.KeyVault.Secrets;

namespace Ume_Chat_KeyVaultProvider;

internal class AzureKeyVaultGateway(SecretClient secretClient) : IKeyVaultGateway
{
    public async Task<Response<KeyVaultSecret>> GetSecretAsync(string secretName, string keyVaultUrl)
    {
        return await secretClient.GetSecretAsync(secretName);
    }
}
