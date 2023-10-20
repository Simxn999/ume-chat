using Azure;
using Azure.Security.KeyVault.Secrets;

namespace KeyVaultProvider;

public interface IKeyVaultGateway
{
    Task<Response<KeyVaultSecret>> GetSecretAsync(string secretName, string keyVaultUrl);
}
