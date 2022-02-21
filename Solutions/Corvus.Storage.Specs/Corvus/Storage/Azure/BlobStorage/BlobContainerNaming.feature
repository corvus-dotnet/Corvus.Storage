Feature: BlobContainerNaming
	As a developer using blob storage
	In order to satisfy Azure Blob Storage's restrictions around container naming
	I need to be able to convert logical names into container names that do not break the rules


# We test a couple of well-known tenants used in Marain.Tenancy, to verify that we get the expected
# container names


# In Corvus tenancy, the root tenant id is f26450ab1668784bb327951c8b08f347. The logical container
# name used by Marain.Tenancy for each per-tenant container is 'corvustenancy', making the tenanted
# name 'f26450ab1668784bb327951c8b08f347-corvustenancy'. The hashed version of this name is the name
# of the container in which any Marain.Tenancy installation stores children of the root tenant,
# is 'cce7b3deef3998aad88f5f0116f922a94e7cb6c4'.
Scenario: Well known root tenant container name becomes well known root container name
	Given a logical blob container name of 'f26450ab1668784bb327951c8b08f347-corvustenancy'
	When the logical name is passed to AzureStorageBlobContainerNaming.HashAndEncodeContainerName
	Then the resulting blob storage container name is 'cce7b3deef3998aad88f5f0116f922a94e7cb6c4'

Scenario: Well known Service Tenants container name becomes well known root container name
	Given a logical blob container name of '3633754ac4c9be44b55bfe791b1780f1-corvustenancy'
	When the logical name is passed to AzureStorageBlobContainerNaming.HashAndEncodeContainerName
	Then the resulting blob storage container name is '513162c27e77e52411ececa40f1e615455b01fc5'

Scenario: Well known Client Tenants container name becomes well known root container name
	Given a logical blob container name of '75b9261673c2714681f14c97bc0439fb-corvustenancy'
	When the logical name is passed to AzureStorageBlobContainerNaming.HashAndEncodeContainerName
	Then the resulting blob storage container name is '8f33344016814e24b39748c7d33e9da6f7772875'
