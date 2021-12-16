Feature: BlobContainerConfiguration
    As the person responsible for deploying and configuring an application that use Azure Blob Storage
    I need to be able to supply the necessary details and credentials in various different ways
    So that I can connect to the correct storage account while meeting the security requirements of my application

# Scenarios to check
#
# Just the connection string (with either embedded credentials, or:
#   Plaintext ConnectionString
#   Legacy plaintext connection string in AccountName
#   Connection string in key vault using ambient service identity
#   Connection string in key vault accessed using distinct credentials read from a different key vault accessed using ambient service identity
#
# Account name with secret
#
#
# Account name, no secret, Azure AD auth
#  

Scenario: Connection string in configuration
    Given BlobContainerConfiguration configuration of
        """
        {
          "config": {
            "Container": "MyContainer",
            "ConnectionStringPlainText": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
          }
        }
        """
    When I get a blob storage container for 'config' as 'c1'
    And I get a blob storage container for 'config' as 'c2'
    Then the storage client endpoint in 'c1' should be 'http://127.0.0.1:10000/devstoreaccount1/MyContainer'
    # Would like to test that the AccountKey is also present, but there isn't a straightforward way to do that.
    And the BlobContainerClient for containers 'c1' and 'c2' should be the same instance

Scenario: Account name and managed identity
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
    When I get a blob storage container for 'config' as 'c1'
    And I get a blob storage container for 'config' as 'c2'
    Then the storage client endpoint in 'c1' should be 'https://myaccount.blob.core.windows.net/MyContainer'
    And the BlobContainerClient for containers 'c1' and 'c2' should be the same instance
    And the BlobContainerConfiguration.ClientIdentity from 'config' should have been passed to the token credential source
    # Would like to test that the TokenCredential returned by the token credential source was used, but there isn't a straightforward way to do that.
