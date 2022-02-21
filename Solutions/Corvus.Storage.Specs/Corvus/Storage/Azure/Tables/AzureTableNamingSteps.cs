// <copyright file="AzureTableNamingSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Storage.Azure.TableStorage;

using NUnit.Framework;

using TechTalk.SpecFlow;

namespace Corvus.Storage.Azure.Tables;

[Binding]
internal class AzureTableNamingSteps
{
    private string? logicalTableName;
    private string? resultingTableName;

    [Given("a logical table name of '([^']*)'")]
    public void GivenALogicalTableNameOf(string logicalTableName)
    {
        this.logicalTableName = logicalTableName;
    }

    [When(@"the logical name is passed to AzureTableNaming\.HashAndEncodeContainerName")]
    public void WhenTheLogicalNameIsPassedToAzureTableNaming_HashAndEncodeContainerName()
    {
        this.resultingTableName =
            AzureTableNaming.HashAndEncodeTableName(this.logicalTableName!);
    }

    [Then("the resulting table name is '([^']*)'")]
    public void ThenTheResultingTableNameIs(string expectedPhysicalTableName)
    {
        Assert.AreEqual(expectedPhysicalTableName, this.resultingTableName);
    }
}