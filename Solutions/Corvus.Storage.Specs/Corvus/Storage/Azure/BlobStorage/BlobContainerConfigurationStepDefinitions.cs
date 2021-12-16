// <copyright file="BlobContainerConfigurationStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Azure.Storage.Blobs;

using Corvus.Storage.Azure.BlobStorage.Internal;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using TechTalk.SpecFlow;

namespace Corvus.Storage.Azure.BlobStorage
{
    [Binding]
    public sealed class BlobContainerConfigurationStepDefinitions : IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IBlobContainerSourceFromDynamicConfiguration containerSource;
        private readonly Dictionary<string, BlobContainerConfiguration> configurations = new ();
        private readonly Dictionary<string, BlobContainerClient> containers = new ();
        private readonly TokenCredentialSourceBindings tokenCredentialSourceBindings;

        private string? validationMessage;
        private BlobContainerConfigurationTypes validatedType;

        public BlobContainerConfigurationStepDefinitions(
            TokenCredentialSourceBindings tokenCredentialSourceBindings)
        {
            this.tokenCredentialSourceBindings = tokenCredentialSourceBindings;

            ServiceCollection services = new ();
            services.AddAzureBlobStorageClientSourceFromDynamicConfiguration();
            this.tokenCredentialSourceBindings.AddFakeTokenCredentialSource(services);
            this.serviceProvider = services.BuildServiceProvider();

            this.containerSource = this.serviceProvider.GetRequiredService<IBlobContainerSourceFromDynamicConfiguration>();
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

        [Given("I get a blob storage container for '([^']*)' as '([^']*)'")]
        [When("I get a blob storage container for '([^']*)' as '([^']*)'")]
        public async Task WhenIGetABlobStorageContainerForAs(string configName, string containerName)
        {
            BlobContainerConfiguration config = this.configurations[configName];
            BlobContainerClient container = await this.containerSource.GetStorageContextAsync(config).ConfigureAwait(false);
            this.containers.Add(containerName, container);
        }

        [When("I validate blob storage configuration '([^']*)'")]
        public void WhenIValidateBlobStorageConfiguration(string configName)
        {
            BlobContainerConfiguration config = this.configurations[configName];
            this.validationMessage = BlobContainerConfigurationValidation.Validate(
                config,
                out this.validatedType);
        }

        [When("I get a replacement for a failed blob storage container for '([^']*)' as '([^']*)'")]
        public async Task GivenIRecreatedABlobStorageContainerForAsAsync(string configName, string containerName)
        {
            BlobContainerConfiguration config = this.configurations[configName];
            BlobContainerClient container = await this.containerSource.GetReplacementForFailedStorageContextAsync(config).ConfigureAwait(false);
            this.containers.Add(containerName, container);
        }

        [Then("blob storage configuration validation succeeds")]
        public void ThenValidationSucceeds()
        {
            Assert.IsNull(this.validationMessage);
        }

        [Then("validation determines that the blob storage configuration type is '([^']*)'")]
        public void ThenValidationDeterminesThatTheBlobStorageConfigurationTypeIs(
            string type)
        {
            Assert.AreEqual(Enum.Parse<BlobContainerConfigurationTypes>(type), this.validatedType);
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

        [Then("the BlobContainerClient for containers '([^']*)' and '([^']*)' should be different instances")]
        public void ThenTheBlobContainerClientForContainersAndShouldBeDifferentInstances(
            string containerName1, string containerName2)
        {
            BlobContainerClient container1 = this.containers[containerName1];
            BlobContainerClient container2 = this.containers[containerName2];

            Assert.AreNotSame(container1, container2);
        }

        [Then(@"the BlobContainerConfiguration\.ClientIdentity from '([^']*)' should have been passed to the token credential source")]
        public void ThenTheBlobContainerConfiguration_ClientIdentityShouldHaveBeenPassedToTheTokenCredentialSource(
            string configurationName)
        {
            Assert.AreEqual(1, this.tokenCredentialSourceBindings.IdentityConfigurations.Count);
            Assert.AreSame(
                this.configurations[configurationName].ClientIdentity,
                this.tokenCredentialSourceBindings.IdentityConfigurations[0]);
        }

        [Then(@"the BlobContainerConfiguration\.ClientIdentity from '([^']*)' should have been invalidated")]
        public void ThenTokenCredentialSourceReplacementShouldHaveBeenObtained(
            string configurationName)
        {
            Assert.AreEqual(1, this.tokenCredentialSourceBindings.IdentityConfigurations.Count);
            Assert.AreSame(
                this.configurations[configurationName].ClientIdentity,
                this.tokenCredentialSourceBindings.InvalidatedIdentityConfigurations[0]);
        }
    }
}
