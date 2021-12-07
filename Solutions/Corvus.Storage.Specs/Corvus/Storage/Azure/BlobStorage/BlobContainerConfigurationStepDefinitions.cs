// <copyright file="BlobContainerConfigurationStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Azure.Storage.Blobs;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using TechTalk.SpecFlow;

namespace Corvus.Storage.Azure.BlobStorage
{
    [Binding]
    public class BlobContainerConfigurationStepDefinitions : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IBlobContainerSourceByConfiguration containerSource;
        private readonly Dictionary<string, BlobContainerConfiguration> configurations = new ();
        private readonly Dictionary<string, BlobContainerClient> containers = new ();

        public BlobContainerConfigurationStepDefinitions()
        {
            ServiceCollection services = new ();
            services.AddAzureBlobStorageClientSource();
            this.serviceProvider = services.BuildServiceProvider();

            this.containerSource = this.serviceProvider.GetRequiredService<IBlobContainerSourceByConfiguration>();
        }

        public void Dispose()
        {
            this.serviceProvider.Dispose();
        }

        [Given("BlobContainerConfiguration configuration of")]
        public void GivenBlobContainerConfigurationConfigurationOf(string configText)
        {
            ConfigurationBuilder cb = new ();
            cb.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configText)));
            IConfiguration configuration = cb.Build();

            foreach (IConfigurationSection section in configuration.GetChildren())
            {
                string configName = section.Key;
                BlobContainerConfiguration config = section.Get<BlobContainerConfiguration>();
                this.configurations.Add(configName, config);
            }
        }

        [When("I get a blob storage container for '([^']*)' as '([^']*)'")]
        public async Task WhenIGetABlobStorageContainerForAs(string configName, string containerName)
        {
            BlobContainerConfiguration config = this.configurations[configName];
            BlobContainerClient container = await this.containerSource.GetStorageContextAsync(config);
            this.containers.Add(containerName, container);
        }

        [Then("the storage client endpoint in '([^']*)' should be '([^']*)'")]
        public void ThenTheStorageClientEndpointInShouldBe(string containerName, string expectedEndpoint)
        {
            BlobContainerClient container = this.containers[containerName];
            Assert.AreEqual(expectedEndpoint, container.Uri.ToString());
        }

        [Then("the BlobContainerClient for containers '([^']*)' and '([^']*)' should be the same instance")]
        public void ThenTheBlobContainerClientForContainersAndShouldBeTheSameInstance(
            string containerName1, string containerName2)
        {
            BlobContainerClient container1 = this.containers[containerName1];
            BlobContainerClient container2 = this.containers[containerName2];

            Assert.AreSame(container1, container2);
        }
    }
}
