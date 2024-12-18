// <copyright file="CosmosContainerConfigurationStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Corvus.Storage.Azure.Cosmos.Internal;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;
using Reqnroll;

namespace Corvus.Storage.Azure.Cosmos
{
    [Binding]
    public sealed class CosmosContainerConfigurationStepDefinitions : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly ICosmosContainerSourceFromDynamicConfiguration containerSource;
        private readonly Dictionary<string, CosmosContainerConfiguration> configurations = new();
        private readonly Dictionary<string, Container> containers = new();

        private string? validationMessage;
        private CosmosContainerConfigurationTypes validatedType;

        public CosmosContainerConfigurationStepDefinitions()
        {
            ServiceCollection services = new();
            services.AddCosmosContainerSourceFromDynamicConfiguration();
            this.serviceProvider = services.BuildServiceProvider();

            this.containerSource = this.serviceProvider.GetRequiredService<ICosmosContainerSourceFromDynamicConfiguration>();
        }

        public void Dispose()
        {
            this.serviceProvider.Dispose();
        }

        [Given("CosmosContainerConfiguration of")]
        public void GivenCosmosContainerConfigurationOf(string configText)
        {
            ConfigurationBuilder cb = new();
            cb.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configText)));
            IConfiguration configuration = cb.Build();

            foreach (IConfigurationSection section in configuration.GetChildren())
            {
                string configName = section.Key;
                CosmosContainerConfiguration config = section.Get<CosmosContainerConfiguration>()!;
                this.configurations.Add(configName, config);
            }
        }

        [When("I get a Cosmos DB container for '([^']*)' as '([^']*)'")]
        public async Task WhenIGetACosmosDBContainer(string configName, string containerName)
        {
            CosmosContainerConfiguration config = this.configurations[configName]!;
            Container container = await this.containerSource.GetStorageContextAsync(config).ConfigureAwait(false);
            this.containers.Add(containerName, container);
        }

        [When("I validate Cosmos DB storage configuration '([^']*)'")]
        public void WhenIValidateBlobStorageConfiguration(string configName)
        {
            CosmosContainerConfiguration config = this.configurations[configName];
            this.validationMessage = CosmosContainerConfigurationValidation.Validate(
                config,
                out this.validatedType);
        }

        [Then("Cosmos DB storage configuration validation succeeds")]
        public void ThenValidationSucceeds()
        {
            Assert.IsNull(this.validationMessage);
        }

        [Then("validation determines that the Cosmos DB storage configuration type is '([^']*)'")]
        public void ThenValidationDeterminesThatTheBlobStorageConfigurationTypeIs(
            string type)
        {
            Assert.AreEqual(Enum.Parse<CosmosContainerConfigurationTypes>(type), this.validatedType);
        }

        [Then(@"the CosmosClient\.Endpoint in '([^']*)' should be '([^']*)'")]
        public void ThenTheCosmosClient_EndpointShouldBe(string containerName, string expectedEndpoint)
        {
            Container container = this.containers[containerName];
            Assert.AreEqual(expectedEndpoint, container.Database.Client.Endpoint.ToString());
        }

        [Then("the Cosmos Database in '([^']*)' is '([^']*)'")]
        public void ThenTheCosmosDatabaseInIs(string containerName, string expectedDatabase)
        {
            Container container = this.containers[containerName];
            Assert.AreEqual(expectedDatabase, container.Database.Id);
        }

        [Then("the Cosmos Container in '([^']*)' is '([^']*)'")]
        public void ThenTheCosmosContainerInIs(string containerName, string expectedContainer)
        {
            Container container = this.containers[containerName];
            Assert.AreEqual(expectedContainer, container.Id);
        }

        [Then("the CosmosClient for containers '([^']*)' and '([^']*)' should be the same instance")]
        public void ThenTheCosmosClientForContainersAndShouldBeTheSameInstance(
            string containerName1, string containerName2)
        {
            Container container1 = this.containers[containerName1];
            Container container2 = this.containers[containerName2];

            CosmosClient client1 = container1.Database.Client;
            CosmosClient client2 = container2.Database.Client;

            Assert.AreSame(client1, client2);
        }
    }
}