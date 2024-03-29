﻿using Microsoft.Extensions.Configuration;

namespace KeyVaultProvider;

internal class ConfigurationSource(IConfiguration config, string? azureKeyVaultUrl, IKeyVaultGateway? keyVaultGateway) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new KeyVaultConfigurationProvider(config, azureKeyVaultUrl, keyVaultGateway);
    }
}
