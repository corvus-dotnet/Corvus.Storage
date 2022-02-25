// <copyright file="TableClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Azure.Core;
using Azure.Data.Tables;

using Corvus.Identity.ClientAuthentication.Azure;

namespace Corvus.Storage.Azure.TableStorage.Internal;

/// <summary>
/// A factory for a <see cref="TableClient"/>.
/// </summary>
internal class TableClientFactory :
    CachingStorageContextFactory<TableClient, TableConfiguration, TableClientOptions>,
    ITableSourceFromDynamicConfiguration
{
    /// <summary>
    /// Creates a <see cref="TableClientFactory"/>.
    /// </summary>
    /// <param name="serviceProvider">
    /// Provides access to dependencies that are only needed in certain scenarios, and which
    /// we don't want to cause a DI initialization failure for if they are absent. (We depend
    /// on <see cref="IServiceIdentityAzureTokenCredentialSource"/>, but only in certain
    /// scenarios.)
    /// </param>
    public TableClientFactory(
        IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected async override ValueTask<TableClient> CreateContextAsync(
        TableConfiguration configuration,
        TableClientOptions? connectionOptions,
        CancellationToken cancellationToken)
    {
        TableServiceClient tableServiceClient =
                await this.CreateTableServiceClientAsync(
                    configuration,
                    connectionOptions,
                    cancellationToken)
                .ConfigureAwait(false);

        return tableServiceClient.GetTableClient(configuration.TableName);
    }

    /// <inheritdoc/>
    protected override string GetCacheKeyForConfiguration(
        TableConfiguration contextConfiguration)
    {
        // TODO: there are many options for configuration, and we need to work out a sound way
        // to reduce that reliably to a cache key.
        // This is a placeholder that kind of works but is bad for a lot of reasons
        // https://github.com/corvus-dotnet/Corvus.Storage/issues/3
        return System.Text.Json.JsonSerializer.Serialize(contextConfiguration);
    }

    /// <inheritdoc/>
    protected override void InvalidateForConfiguration(
        TableConfiguration configuration,
        TableClientOptions? connectionOptions,
        CancellationToken cancellationToken)
    {
        this.InvalidateCredentials(configuration.ClientIdentity);
        this.InvalidateCredentials(configuration.ConnectionStringInKeyVault?.VaultClientIdentity);
        this.InvalidateCredentials(configuration.AccessKeyInKeyVault?.VaultClientIdentity);
    }

    private static Uri AccountUri(string accountName)
        => new($"https://{accountName}.table.core.windows.net");

    private static ValueTask<TableServiceClient> ClientFromConnectionStringAsPlainText(
        TableConfiguration configuration,
        TableClientOptions? clientOptions)
    {
        return new ValueTask<TableServiceClient>(
            new TableServiceClient(configuration.ConnectionStringPlainText, clientOptions));
    }

    private static ValueTask<TableServiceClient> AccountNameAndAccessKeyAsPlainText(
        TableConfiguration configuration,
        TableClientOptions? clientOptions)
    {
        return new ValueTask<TableServiceClient>(
            new TableServiceClient(
                AccountUri(configuration.AccountName!),
                new TableSharedKeyCredential(configuration.AccountName, configuration.AccessKeyPlainText!),
                clientOptions));
    }

    private async ValueTask<TableServiceClient> ClientFromConnectionStringInKeyVault(
        TableConfiguration configuration,
        TableClientOptions? clientOptions,
        CancellationToken cancellationToken)
    {
        string? connectionString =
            await this.GetKeyVaultSecretFromConfigAsync(
                configuration.ConnectionStringInKeyVault!,
                cancellationToken)
                .ConfigureAwait(false);
        return new TableServiceClient(
                connectionString,
                clientOptions);
    }

    private async ValueTask<TableServiceClient> AccountNameAndAccessKeyInKeyVault(
        TableConfiguration configuration,
        TableClientOptions? clientOptions,
        CancellationToken cancellationToken)
    {
        string? accessKey = await this.GetKeyVaultSecretFromConfigAsync(
            configuration.AccessKeyInKeyVault!,
            cancellationToken)
            .ConfigureAwait(false);

        if (accessKey is null)
        {
            throw new InvalidOperationException($"Failed to get secret {configuration.AccessKeyInKeyVault!.SecretName} from {configuration.AccessKeyInKeyVault!.VaultName}");
        }

        return new TableServiceClient(
            AccountUri(configuration.AccountName!),
            new TableSharedKeyCredential(configuration.AccountName, accessKey),
            clientOptions);
    }

    private async ValueTask<TableServiceClient> ClientFromAccountNameAndClientIdentity(
        TableConfiguration configuration,
        TableClientOptions? clientOptions,
        CancellationToken cancellationToken)
    {
        IAzureTokenCredentialSource credentialSource =
            await this.AzureTokenCredentialSourceFromConfig.CredentialSourceForConfigurationAsync(
                configuration.ClientIdentity!,
                cancellationToken)
            .ConfigureAwait(false);
        TokenCredential tokenCredential = await credentialSource.GetTokenCredentialAsync(cancellationToken)
            .ConfigureAwait(false);
        return new TableServiceClient(
            AccountUri(configuration.AccountName!),
            tokenCredential,
            clientOptions);
    }

    private async Task<TableServiceClient> CreateTableServiceClientAsync(
        TableConfiguration configuration,
        TableClientOptions? clientOptions,
        CancellationToken cancellationToken)
    {
        string? validationMessage = TableConfigurationValidation.Validate(
            configuration, out TableConfigurationTypes configurationType);
        if (validationMessage is not null)
        {
            throw new ArgumentException(
                "Invalid TableConfiguration: " + validationMessage,
                nameof(configuration));
        }

        ValueTask<TableServiceClient> r = configurationType switch
        {
            TableConfigurationTypes.ConnectionStringAsPlainText =>
                ClientFromConnectionStringAsPlainText(configuration, clientOptions),

            TableConfigurationTypes.ConnectionStringInKeyVault =>
                this.ClientFromConnectionStringInKeyVault(configuration, clientOptions, cancellationToken),

            TableConfigurationTypes.AccountNameAndAccessKeyAsPlainText =>
                AccountNameAndAccessKeyAsPlainText(configuration, clientOptions),

            TableConfigurationTypes.AccountNameAndAccessKeyInKeyVault =>
                this.AccountNameAndAccessKeyInKeyVault(
                    configuration, clientOptions, cancellationToken),

            TableConfigurationTypes.AccountNameAndClientIdentity =>
                this.ClientFromAccountNameAndClientIdentity(
                    configuration, clientOptions, cancellationToken),

            _ => throw new InvalidOperationException($"Unknown configuration type {configurationType}"),
        };

        return await r.ConfigureAwait(false);
    }
}