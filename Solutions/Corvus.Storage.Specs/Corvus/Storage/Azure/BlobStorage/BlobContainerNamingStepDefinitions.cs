// <copyright file="BlobContainerNamingStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using NUnit.Framework;

using TechTalk.SpecFlow;

namespace Corvus.Storage.Azure.BlobStorage
{
    [Binding]
    public class BlobContainerNamingStepDefinitions
    {
        private string? logicalContainerName;
        private string? resultingContainerName;

        [Given("a logical blob container name of '([^']*)'")]
        public void GivenALogicalBlobContainerNameOf(string logicalContainerName)
        {
            this.logicalContainerName = logicalContainerName;
        }

        [When(@"the logical name is passed to AzureStorageBlobContainerNaming\.HashAndEncodeContainerName")]
        public void WhenTheLogicalNameIsPassedToAzureStorageBlobContainerNaming_HashAndEncodeContainerName()
        {
            this.resultingContainerName =
                AzureStorageBlobContainerNaming.HashAndEncodeBlobContainerName(this.logicalContainerName!);
        }

        [Then("the resulting blob storage container name is '([^']*)'")]
        public void ThenTheResultingBlobStorageContainerNameIs(string expectedPhysicalContainerName)
        {
            Assert.AreEqual(expectedPhysicalContainerName, this.resultingContainerName);
        }
    }
}