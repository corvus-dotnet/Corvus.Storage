// <copyright file="TableConfigurationValidation.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.TableStorage.Internal;

/// <summary>
/// Checks <see cref="TableConfiguration"/> instances for validity.
/// </summary>
internal static class TableConfigurationValidation
{
    /// <summary>
    /// Checks a <see cref="TableConfiguration"/> for validity.
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
        TableConfiguration configuration,
        out TableConfigurationTypes type)
    {
        type = default;

        if (configuration is null)
        {
            return "must not be null";
        }

        HashSet<TableConfigurationTypes> indicatedConfigTypes = new();

        bool accountNamePresent = !string.IsNullOrWhiteSpace(configuration.AccountName);
        bool accessKeyPlainTextPresent = !string.IsNullOrWhiteSpace(configuration.AccessKeyPlainText);
        bool accessKeyInKeyVaultPresent = configuration.AccessKeyInKeyVault is not null;
        bool connectionStringPlainTextPresent = !string.IsNullOrWhiteSpace(configuration.ConnectionStringPlainText);
        bool connectionStringInKeyVaultPresent = configuration.ConnectionStringInKeyVault is not null;
        bool clientIdentityPresent = configuration.ClientIdentity is not null;

        void MatchTypeIf(bool match, TableConfigurationTypes type)
        {
            if (match)
            {
                indicatedConfigTypes.Add(type);
            }
        }

        MatchTypeIf(
            accountNamePresent && accessKeyPlainTextPresent,
            TableConfigurationTypes.AccountNameAndAccessKeyAsPlainText);
        MatchTypeIf(
            accountNamePresent && accessKeyInKeyVaultPresent,
            TableConfigurationTypes.AccountNameAndAccessKeyInKeyVault);
        MatchTypeIf(
            accountNamePresent && clientIdentityPresent,
            TableConfigurationTypes.AccountNameAndClientIdentity);
        MatchTypeIf(
            connectionStringPlainTextPresent,
            TableConfigurationTypes.ConnectionStringAsPlainText);
        MatchTypeIf(
            connectionStringInKeyVaultPresent,
            TableConfigurationTypes.ConnectionStringInKeyVault);

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