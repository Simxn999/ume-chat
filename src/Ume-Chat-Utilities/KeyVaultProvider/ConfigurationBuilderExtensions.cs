﻿using Microsoft.Extensions.Configuration;

namespace KeyVaultProvider;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddAzureKeyVaultWithReferenceSupport(this IConfigurationBuilder builder,
                                                                             string? azureKeyVaultUrl = null,
                                                                             IKeyVaultGateway? keyVaultGateway = null)
    {
        return builder.Add(new ConfigurationSource(builder.Build(), azureKeyVaultUrl, keyVaultGateway));
    }
}
