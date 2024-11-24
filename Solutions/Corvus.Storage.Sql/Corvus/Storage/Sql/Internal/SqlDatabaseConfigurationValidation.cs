// <copyright file="SqlDatabaseConfigurationValidation.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Sql.Internal;

/// <summary>
/// Checks <see cref="SqlDatabaseConfiguration"/> instances for validity.
/// </summary>
internal static class SqlDatabaseConfigurationValidation
{
    /// <summary>
    /// Checks a <see cref="SqlDatabaseConfiguration"/> for validity.
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
        SqlDatabaseConfiguration? configuration,
        out SqlDatabaseConfigurationTypes type)
    {
        type = default;

        if (configuration is null)
        {
            return "must not be null";
        }

        HashSet<SqlDatabaseConfigurationTypes> indicatedConfigTypes = new();

        bool connectionStringPlainTextPresent = !string.IsNullOrWhiteSpace(configuration.ConnectionStringPlainText);
        bool connectionStringInKeyVaultPresent = configuration.ConnectionStringInKeyVault is not null;

        void MatchTypeIf(bool match, SqlDatabaseConfigurationTypes type)
        {
            if (match)
            {
                indicatedConfigTypes.Add(type);
            }
        }

        MatchTypeIf(
            connectionStringPlainTextPresent,
            SqlDatabaseConfigurationTypes.ConnectionStringAsPlainText);
        MatchTypeIf(
            connectionStringInKeyVaultPresent,
            SqlDatabaseConfigurationTypes.ConnectionStringInKeyVault);

        switch (indicatedConfigTypes.Count)
        {
            case 0:
                return "unable to determine SQL database configuration type because no suitable properties have been set";

            case 1:
                type = indicatedConfigTypes.Single();
                break;

            default:
                return $"blob configuration type is ambiguous because the properties set are for {string.Join(", ", indicatedConfigTypes)}";
        }

        return null;
    }
}