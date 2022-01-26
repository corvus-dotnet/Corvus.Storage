// <copyright file="BlobContainerConfigurationValidation.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace Corvus.Storage.Azure.BlobStorage.Internal;

/// <summary>
/// Checks <see cref="BlobContainerConfiguration"/> instances for validity.
/// </summary>
internal static class BlobContainerConfigurationValidation
{
    /// <summary>
    /// Checks a <see cref="BlobContainerConfiguration"/> for validity.
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
        BlobContainerConfiguration configuration,
        out BlobContainerConfigurationTypes type)
    {
        type = default;

        if (configuration is null)
        {
            return "must not be null";
        }

        HashSet<BlobContainerConfigurationTypes> indicatedConfigTypes = new ();

        bool accountNamePresent = !string.IsNullOrWhiteSpace(configuration.AccountName);
        bool accessKeyPlainTextPresent = !string.IsNullOrWhiteSpace(configuration.AccessKeyPlainText);
        bool accessKeyInKeyVaultPresent = configuration.AccessKeyInKeyVault is not null;
        bool connectionStringPlainTextPresent = !string.IsNullOrWhiteSpace(configuration.ConnectionStringPlainText);
        bool connectionStringInKeyVaultPresent = configuration.ConnectionStringInKeyVault is not null;
        bool clientIdentityPresent = configuration.ClientIdentity is not null;

        void MatchTypeIf(bool match, BlobContainerConfigurationTypes type)
        {
            if (match)
            {
                indicatedConfigTypes.Add(type);
            }
        }

        MatchTypeIf(
            accountNamePresent && accessKeyPlainTextPresent,
            BlobContainerConfigurationTypes.AccountNameAndAccessKeyAsPlainText);
        MatchTypeIf(
            accountNamePresent && accessKeyInKeyVaultPresent,
            BlobContainerConfigurationTypes.AccountNameAndAccessKeyInKeyVault);
        MatchTypeIf(
            accountNamePresent && clientIdentityPresent,
            BlobContainerConfigurationTypes.AccountNameAndClientIdentity);
        MatchTypeIf(
            connectionStringPlainTextPresent,
            BlobContainerConfigurationTypes.ConnectionStringAsPlainText);
        MatchTypeIf(
            connectionStringInKeyVaultPresent,
            BlobContainerConfigurationTypes.ConnectionStringInKeyVault);

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