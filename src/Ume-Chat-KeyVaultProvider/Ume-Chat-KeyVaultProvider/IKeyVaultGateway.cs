using Azure;
using Azure.Security.KeyVault.Secrets;

namespace Ume_Chat_KeyVaultProvider;

public interface IKeyVaultGateway
{
    Task<Response<KeyVaultSecret>> GetSecretAsync(string secretName, string keyVaultUrl);
}