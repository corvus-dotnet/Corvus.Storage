// <copyright file="SqlConnectionFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Azure;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;

using Corvus.Identity.ClientAuthentication.Azure;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Corvus.Storage.Sql.Internal;

/// <summary>
/// A factory for a <see cref="SqlConnection"/>.
/// </summary>
internal class SqlConnectionFactory :
    ISqlConnectionFromDynamicConfiguration
{
    private readonly IServiceProvider serviceProvider;
    private IAzureTokenCredentialSourceFromDynamicConfiguration? azureTokenCredentialSourceFromConfig;

    /// <summary>
    /// Creates a <see cref="SqlConnectionFactory"/>.
    /// </summary>
    /// <param name="serviceProvider">
    /// Required by the base class.
    /// </param>
    public SqlConnectionFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets an <see cref="IAzureTokenCredentialSourceFromDynamicConfiguration"/> from DI.
    /// (This retrieves this on demand, so if DI has not been configured so as to supply
    /// this, it will fail on first use of this property, rather than at the point where DI
    /// constructs us.)
    /// </summary>
    /// <remarks>
    /// TODO: duped from CachingStorageContextFactory. Factor out, and also consider a caching
    /// key vault client.
    /// </remarks>
    private IAzureTokenCredentialSourceFromDynamicConfiguration AzureTokenCredentialSourceFromConfig
        => this.azureTokenCredentialSourceFromConfig ??= this.serviceProvider.GetRequiredService<IAzureTokenCredentialSourceFromDynamicConfiguration>();

    /// <inheritdoc/>
    public async ValueTask<SqlConnection> GetStorageContextAsync(
        SqlDatabaseConfiguration configuration,
        object? connectionOptions,
        CancellationToken cancellationToken)
    {
        string? validationMessage = SqlDatabaseConfigurationValidation.Validate(
            configuration, out SqlDatabaseConfigurationTypes configurationType);
        if (validationMessage is not null)
        {
            throw new ArgumentException(
                "Invalid SqlDatabaseConfiguration: " + validationMessage,
                nameof(configuration));
        }

        ValueTask<SqlConnection> r = configurationType switch
        {
            SqlDatabaseConfigurationTypes.ConnectionStringAsPlainText =>
                ConnectionFromConnectionStringAsPlainText(configuration),

            SqlDatabaseConfigurationTypes.ConnectionStringInKeyVault =>
                this.ConnectionFromConnectionStringInKeyVault(configuration, cancellationToken),

            _ => throw new InvalidOperationException($"Unknown configuration type {configurationType}"),
        };

        return await r.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<SqlConnection> GetReplacementForFailedStorageContextAsync(
        SqlDatabaseConfiguration configuration,
        object? connectionOptions,
        CancellationToken cancellationToken = default)
    {
        // TODO: refactor? Duped from CachingStorageContextFactory
        if (configuration.ConnectionStringInKeyVault?.VaultClientIdentity is not null)
        {
            this.AzureTokenCredentialSourceFromConfig.InvalidateFailedAccessToken(configuration.ConnectionStringInKeyVault.VaultClientIdentity);
        }

        //// TODO: invalidation for SQL client ID when we support it.

        return await this.GetStorageContextAsync(
            configuration, connectionOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    private static ValueTask<SqlConnection> ConnectionFromConnectionStringAsPlainText(
        SqlDatabaseConfiguration configuration)
    {
        return new ValueTask<SqlConnection>(new SqlConnection(configuration.ConnectionStringPlainText));
    }

    private async ValueTask<SqlConnection> ConnectionFromConnectionStringInKeyVault(
        SqlDatabaseConfiguration configuration, CancellationToken cancellationToken)
    {
        KeyVaultSecretConfiguration secretConfiguration = configuration.ConnectionStringInKeyVault!;
        string? connectionString =
            await this.GetKeyVaultSecretFromConfigAsync(
                secretConfiguration,
                cancellationToken)
                .ConfigureAwait(false);

        if (connectionString is null)
        {
            throw new InvalidOperationException($"Secret {secretConfiguration.SecretName} not found in vault {secretConfiguration.VaultName}");
        }

        return new SqlConnection(connectionString);
    }

    // TODO: refactor out of CachingStorageContextFactory so we can use this even in providers
    // like this that don't want the context caching.
    private async ValueTask<string?> GetKeyVaultSecretFromConfigAsync(
        KeyVaultSecretConfiguration secretConfiguration,
        CancellationToken cancellationToken)
    {
        // If no identity for the key vault is specified we use the ambient service
        // identity. Otherwise, we use the identity configuration supplied.
        IAzureTokenCredentialSource credentialSource = secretConfiguration.VaultClientIdentity is null
            ? this.serviceProvider.GetRequiredService<IServiceIdentityAzureTokenCredentialSource>()
            : await this.AzureTokenCredentialSourceFromConfig
                .CredentialSourceForConfigurationAsync(secretConfiguration.VaultClientIdentity, cancellationToken)
                .ConfigureAwait(false);
        TokenCredential? keyVaultCredentials = await credentialSource.GetTokenCredentialAsync(cancellationToken)
            .ConfigureAwait(false);
        if (keyVaultCredentials is not null)
        {
            var keyVaultUri = new Uri($"https://{secretConfiguration.VaultName}.vault.azure.net/");
            var keyVaultClient = new SecretClient(keyVaultUri, keyVaultCredentials);

            Response<KeyVaultSecret> accountKeyResponse = await keyVaultClient.GetSecretAsync(
                secretConfiguration.SecretName,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return accountKeyResponse.Value.Value;
        }

        return null;
    }
}
