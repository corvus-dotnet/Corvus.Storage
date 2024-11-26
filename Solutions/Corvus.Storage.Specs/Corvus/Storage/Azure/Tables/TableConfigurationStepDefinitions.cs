// <copyright file="TableConfigurationStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Azure.Data.Tables;

using Corvus.Storage.Azure.BlobStorage;
using Corvus.Storage.Azure.TableStorage;
using Corvus.Storage.Azure.TableStorage.Internal;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;
using Reqnroll;

namespace Corvus.Storage.Azure.Tables;

[Binding]
public class TableConfigurationStepDefinitions
{
    private readonly Dictionary<string, TableConfiguration> configurations = new();
    private readonly Dictionary<string, TableClient> tableClients = new();
    private readonly TokenCredentialSourceBindings tokenCredentialSourceBindings;
    private readonly ITableSourceFromDynamicConfiguration tableSource;

    private string? validationMessage;
    private TableConfigurationTypes validatedType;

    public TableConfigurationStepDefinitions(TokenCredentialSourceBindings tokenCredentialSourceBindings)
    {
        this.tokenCredentialSourceBindings = tokenCredentialSourceBindings;

        ServiceCollection services = new();
        services.AddAzureTableClientSourceFromDynamicConfiguration();
        this.tokenCredentialSourceBindings.AddFakeTokenCredentialSource(services);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        this.tableSource = serviceProvider.GetRequiredService<ITableSourceFromDynamicConfiguration>();
    }

    [Given("TableConfiguration configuration of")]
    public void GivenTableConfigurationConfigurationOf(string configText)
    {
        ConfigurationBuilder cb = new();
        cb.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configText)));
        IConfiguration configuration = cb.Build();

        foreach (IConfigurationSection section in configuration.GetChildren())
        {
            string configName = section.Key;
            TableConfiguration config = section.Get<TableConfiguration>()!;
            this.configurations.Add(configName, config);
        }
    }

    [Given("I get a table client for '([^']*)' as '([^']*)'")]
    [When("I get a table client for '([^']*)' as '([^']*)'")]
    public async Task WhenIGetATableClientForAs(string configName, string tableName)
    {
        TableConfiguration config = this.configurations[configName]!;
        TableClient tableClient = await this.tableSource.GetStorageContextAsync(config).ConfigureAwait(false);
        this.tableClients.Add(tableName, tableClient);
    }

    [When("I validate table configuration '([^']*)'")]
    public void WhenIValidateTableConfiguration(string configName)
    {
        TableConfiguration config = this.configurations[configName];
        this.validationMessage = TableConfigurationValidation.Validate(
            config,
            out this.validatedType);
    }

    [When("I get a replacement for a failed table client for '([^']*)' as '([^']*)'")]
    public async Task WhenIGetAReplacementForAFailedTableClientForAsAsync(string configName, string tableName)
    {
        TableConfiguration config = this.configurations[configName];
        TableClient tableClient = await this.tableSource.GetReplacementForFailedStorageContextAsync(config).ConfigureAwait(false);
        this.tableClients.Add(tableName, tableClient);
    }

    [Then("table configuration validation succeeds")]
    public void ThenTableConfigurationValidationSucceeds()
    {
        Assert.IsNull(this.validationMessage);
    }

    [Then("validation determines that the table configuration type is '([^']*)'")]
    public void ThenValidationDeterminesThatTheTableConfigurationTypeIs(string type)
    {
        Assert.AreEqual(Enum.Parse<TableConfigurationTypes>(type), this.validatedType);
    }

    [Then("the storage client endpoint in table client '([^']*)' should specify account '([^']*)' and table '([^']*)'")]
    public void ThenTheStorageClientEndpointInTableClientShouldSpecifyAccountAndTable(
        string tableClientName,
        string expectedAccountName,
        string expectedTableName)
    {
        TableClient tableClient = this.tableClients[tableClientName];
        Assert.AreEqual(expectedAccountName, tableClient.AccountName);
        Assert.AreEqual(expectedTableName, tableClient.Name);
    }

    [Then("the TableClient for tables '([^']*)' and '([^']*)' should be the same instance")]
    public void ThenTheTableClientForTablesAndShouldBeTheSameInstance(
        string tableName1, string tableName2)
    {
        TableClient tableClient1 = this.tableClients[tableName1];
        TableClient tableClient2 = this.tableClients[tableName2];

        Assert.AreSame(tableClient1, tableClient2);
    }

    [Then("the TableClient for tables '([^']*)' and '([^']*)' should be different instances")]
    public void ThenTheTableClientForTablesAndShouldBeDifferentInstances(
        string tableName1, string tableName2)
    {
        TableClient tableClient1 = this.tableClients[tableName1];
        TableClient tableClient2 = this.tableClients[tableName2];

        Assert.AreNotSame(tableClient1, tableClient2);
    }

    [Then(@"the TableConfiguration\.ClientIdentity from '([^']*)' should have been passed to the token credential source")]
    public void ThenTheTableConfiguration_ClientIdentityFromShouldHaveBeenPassedToTheTokenCredentialSource(
        string configurationName)
    {
        Assert.AreEqual(1, this.tokenCredentialSourceBindings.IdentityConfigurations.Count);
        Assert.AreSame(
            this.configurations[configurationName]?.ClientIdentity,
            this.tokenCredentialSourceBindings.IdentityConfigurations[0]);
    }

    [Then(@"the TableConfiguration\.ClientIdentity from '([^']*)' should have been invalidated")]
    public void ThenTheTableConfiguration_ClientIdentityFromShouldHaveBeenInvalidated(
        string configurationName)
    {
        Assert.AreEqual(1, this.tokenCredentialSourceBindings.InvalidatedIdentityConfigurations.Count);
        Assert.AreSame(
            this.configurations[configurationName]?.ClientIdentity,
            this.tokenCredentialSourceBindings.InvalidatedIdentityConfigurations[0]);
    }
}