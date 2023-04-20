// <copyright file="SqlDatabaseConfigurationStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Corvus.Identity.ClientAuthentication.Azure;
using Corvus.Storage.Sql.Internal;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using TechTalk.SpecFlow;

namespace Corvus.Storage.Sql
{
    [Binding]
    public class SqlDatabaseConfigurationStepDefinitions
    {
        private readonly Dictionary<string, SqlDatabaseConfiguration> configurations = new();
        private readonly SqlDatabaseConfiguration configuration = new();
        private readonly Dictionary<string, SqlConnection> connections = new();
        private readonly ServiceProvider serviceProvider;
        private readonly ISqlConnectionFromDynamicConfiguration containerSource;

        private string? validationMessage;
        private SqlDatabaseConfigurationTypes validatedType;

        public SqlDatabaseConfigurationStepDefinitions()
        {
            ServiceCollection services = new();
            services.AddSqlConnectionFromDynamicConfiguration();
            this.serviceProvider = services.BuildServiceProvider();

            this.containerSource = this.serviceProvider.GetRequiredService<ISqlConnectionFromDynamicConfiguration>();
        }

        [Given("a SqlDatabaseConfiguration")]
        public void GivenASqlDatabaseConfiguration()
        {
            Assert.IsNotNull(this.configuration, "Test error - field should be set already");
        }

        [Given("SqlDatabaseConfiguration of")]
        public void GivenCosmosContainerConfigurationOf(string configText)
        {
            ConfigurationBuilder cb = new();
            cb.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configText)));
            IConfiguration configuration = cb.Build();

            foreach (IConfigurationSection section in configuration.GetChildren())
            {
                string configName = section.Key;
                SqlDatabaseConfiguration config = section.Get<SqlDatabaseConfiguration>();
                this.configurations.Add(configName, config);
            }
        }

        [When("I get a SqlConnection for '([^']*)' as '([^']*)'")]
        public async Task WhenIGetASqlConnectionForAs(string configJsonName, string connectionId)
        {
            SqlDatabaseConfiguration config = this.configurations[configJsonName];
            SqlConnection container = await this.containerSource.GetStorageContextAsync(config).ConfigureAwait(false);
            this.connections.Add(connectionId, container);
        }

        [Given(@"a SqlDatabaseConfiguration\.ConnectionStringPlainText of '([^']*)'")]
        public void GivenASqlDatabaseConfiguration_ConnectionStringPlainTextOf(string connectionString)
        {
            this.configuration.ConnectionStringPlainText = connectionString;
        }

        [Given(@"SqlDatabaseConfiguration\.ConnectionStringInKeyVault set to use vault '([^']*)' and secret '([^']*)'")]
        public void GivenSqlDatabaseConfiguration_ConnectionStringInKeyVaultSetToUseVaultAndSecret(
            string vaultName, string secretName)
        {
            this.configuration.ConnectionStringInKeyVault = new()
            {
                VaultName = vaultName,
                SecretName = secretName,
            };
        }

        [Given(@"SqlDatabaseConfiguration\.ClientIdentity set to use '([^']*)'")]
        public void GivenSqlDatabaseConfiguration_ClientIdentitySetToUse(ClientIdentitySourceTypes identitySourceType)
        {
            this.configuration.ClientIdentity = identitySourceType switch
            {
                ClientIdentitySourceTypes.None => null,
                ClientIdentitySourceTypes.SystemAssignedManaged => new ClientIdentityConfiguration
                {
                    IdentitySourceType = identitySourceType,
                },

                _ => throw new NotSupportedException($"Test step does not support ClientIdentitySourceTypes.{identitySourceType}"),
            };
        }

        [When("I validate the SqlDatabaseConfiguration")]
        public void WhenIValidateTheSqlDatabaseConfiguration()
        {
            this.validationMessage = SqlDatabaseConfigurationValidation.Validate(
                this.configuration,
                out this.validatedType);
        }

        [When("I validate a null SqlDatabaseConfiguration")]
        public void WhenIValidateANullSqlDatabaseConfiguration()
        {
            this.validationMessage = SqlDatabaseConfigurationValidation.Validate(
                null!,
                out this.validatedType);
        }

        [Then("the SqlDatabaseConfiguration should succeed, and type should be '([^']*)'")]
        public void ThenTheSqlDatabaseConfigurationShouldSucceedAndTypeShouldBe(string expectedType)
        {
            Assert.IsNull(this.validationMessage);
            Assert.AreEqual(Enum.Parse<SqlDatabaseConfigurationTypes>(expectedType), this.validatedType);
        }

        [Then("the SqlDatabaseConfiguration should be reported as invalid")]
        public void ThenTheSqlDatabaseConfigurationShouldBeReportedAsInvalid()
        {
            Assert.IsNotNull(this.validationMessage);
        }

        [Then(@"the SqlConnection\.ConnectionString in '(.*)' should be '(.*)'")]
        public void ThenTheSqlConnection_ConnectionStringInShouldBe(string connectionId, string connectionString)
        {
            SqlConnection connection = this.connections[connectionId];
            Assert.AreEqual(connectionString, connection.ConnectionString);
        }

        [Then("the SqlConnections named '(.*)' and '(.*)' should be different instances")]
        public void ThenTheSqlConnectionsNamedAndShouldBeDifferentInstances(string connectionId1, string connectionId2)
        {
            SqlConnection connection1 = this.connections[connectionId1];
            SqlConnection connection2 = this.connections[connectionId2];
            Assert.AreNotSame(connection1, connection2);
        }
    }
}