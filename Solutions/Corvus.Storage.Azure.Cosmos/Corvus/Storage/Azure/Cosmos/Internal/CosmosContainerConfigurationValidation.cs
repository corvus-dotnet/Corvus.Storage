// <copyright file="CosmosContainerConfigurationValidation.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.Cosmos.Internal;

/// <summary>
/// Checks <see cref="CosmosContainerConfiguration"/> instances for validity.
/// </summary>
internal static class CosmosContainerConfigurationValidation
{
    /// <summary>
    /// Checks a <see cref="CosmosContainerConfiguration"/> for validity.
    /// </summary>
    /// <param name="configuration">
    /// The configuration to check.
    /// </param>
    /// <param name="type">
    /// Returns the type of configuration the validator has determined this to be.
    /// </param>
    /// <returns>
    /// Null if the configuration is valid. A description of the problem if it is not valid.
    /// </returns>
    internal static string? Validate(
        CosmosContainerConfiguration configuration,
        out CosmosContainerConfigurationTypes type)
    {
        type = default;

        if (configuration is null)
        {
            return "must not be null";
        }

        HashSet<CosmosContainerConfigurationTypes> indicatedConfigTypes = new ();

        bool accountUriPresent = !string.IsNullOrWhiteSpace(configuration.AccountUri);
        bool databasePresent = !string.IsNullOrWhiteSpace(configuration.Database);
        bool accessKeyPlainTextPresent = !string.IsNullOrWhiteSpace(configuration.AccessKeyPlainText);
        bool accessKeyInKeyVaultPresent = configuration.AccessKeyInKeyVault is not null;
        bool connectionStringPlainTextPresent = !string.IsNullOrWhiteSpace(configuration.ConnectionStringPlainText);
        bool connectionStringInKeyVaultPresent = configuration.ConnectionStringInKeyVault is not null;
        bool clientIdentityPresent = configuration.ClientIdentity is not null;

        void MatchTypeIf(bool match, CosmosContainerConfigurationTypes type)
        {
            if (match)
            {
                indicatedConfigTypes.Add(type);
            }
        }

        MatchTypeIf(
            databasePresent && accountUriPresent && accessKeyPlainTextPresent,
            CosmosContainerConfigurationTypes.AccountUriAndAccessKeyAsPlainText);
        MatchTypeIf(
            databasePresent && accountUriPresent && accessKeyInKeyVaultPresent,
            CosmosContainerConfigurationTypes.AccountUriAndAccessKeyInKeyVault);
        MatchTypeIf(
            databasePresent && accountUriPresent && clientIdentityPresent,
            CosmosContainerConfigurationTypes.AccountUriAndClientIdentity);
        MatchTypeIf(
            databasePresent && connectionStringPlainTextPresent,
            CosmosContainerConfigurationTypes.ConnectionStringAsPlainText);
        MatchTypeIf(
            databasePresent && connectionStringInKeyVaultPresent,
            CosmosContainerConfigurationTypes.ConnectionStringInKeyVault);

        switch (indicatedConfigTypes.Count)
        {
            case 0:
                return "unable to determine blob configuration type because no suitable properties have been set";

            case 1:
                type = indicatedConfigTypes.Single();
                break;

            default:
                return $"blob configuration type is ambiguous because the properties set are for {string.Join(", ", indicatedConfigTypes)}";
        }

        return null;
    }
}