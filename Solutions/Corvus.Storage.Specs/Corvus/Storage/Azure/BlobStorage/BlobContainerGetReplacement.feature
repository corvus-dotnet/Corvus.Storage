Feature: BlobContainerGetReplacement
	As a developer using Corvus.Storage
	I need to be able to trigger the recreation of a BlobContainerClient if its credentials stop working
	To support key rotation scenarios

Scenario: Recreate connection with connection string
    Given BlobContainerConfiguration configuration of
        """
        {
          "config": {
            "Container": "MyContainer",
            "ConnectionStringPlainText": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
          }
        }
        """
    And I get a blob storage container for 'config' as 'c1'
    When I get a replacement for a failed blob storage container for 'config' as 'c2'
    Then the storage client endpoint in 'c1' should be 'http://127.0.0.1:10000/devstoreaccount1/MyContainer'
    And the storage client endpoint in 'c2' should be 'http://127.0.0.1:10000/devstoreaccount1/MyContainer'
    # Would like to test that the AccountKey is also present, but there isn't a straightforward way to do that.
    And the BlobContainerClient for containers 'c1' and 'c2' should be different instances


Scenario: Recreate connection with client identity
    Given BlobContainerConfiguration configuration of
        """
        {
          "config": {
            "Container": "MyContainer",
            "AccountName": "myaccount",
            "ClientIdentity": {
              "IdentitySourceType": "Managed"
            },
          }
        }
        """
    And I get a blob storage container for 'config' as 'c1'
    And I reset the fake token credential source
    When I get a replacement for a failed blob storage container for 'config' as 'c2'
    Then the storage client endpoint in 'c1' should be 'https://myaccount.blob.core.windows.net/MyContainer'
    And the BlobContainerClient for containers 'c1' and 'c2' should be different instances
    And the BlobContainerConfiguration.ClientIdentity from 'config' should have been invalidated
    # Would like to test that the TokenCredential returned by the token credential source was used, but there isn't a straightforward way to do that.
