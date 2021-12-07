// <copyright file="CosmosContainerConfigurationStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using TechTalk.SpecFlow;

namespace Corvus.Storage.Azure.Cosmos
{
    [Binding]
    public class CosmosContainerConfigurationStepDefinitions : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly ICosmosContainerSourceByConfiguration containerSource;
        private readonly Dictionary<string, CosmosContainerConfiguration> configurations = new ();
        private readonly Dictionary<string, Container> containers = new ();

        public CosmosContainerConfigurationStepDefinitions()
        {
            ServiceCollection services = new ();
            services.AddCosmosContainerSource();
            this.serviceProvider = services.BuildServiceProvider();

            this.containerSource = this.serviceProvider.GetRequiredService<ICosmosContainerSourceByConfiguration>();
        }

        public void Dispose()
        {
            this.serviceProvider.Dispose();
        }

        [Given("CosmosContainerConfiguration of")]
        public void GivenCosmosContainerConfigurationOf(string configText)
        {
            ConfigurationBuilder cb = new ();
            cb.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configText)));
            IConfiguration configuration = cb.Build();

            foreach (IConfigurationSection section in configuration.GetChildren())
            {
                string configName = section.Key;
                CosmosContainerConfiguration config = section.Get<CosmosContainerConfiguration>();
                this.configurations.Add(configName, config);
            }
        }

        [When("I get a Cosmos DB container for '([^']*)' as '([^']*)'")]
        public async Task WhenIGetACosmosDBContainer(string configName, string containerName)
        {
            CosmosContainerConfiguration config = this.configurations[configName];
            Container container = await this.containerSource.GetStorageContextAsync(config);
            this.containers.Add(containerName, container);
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
