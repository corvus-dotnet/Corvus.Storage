Feature: ValidCosmosContainerConfigurations
	As the person responsible for deploying and configuring an application that uses Azure Cosmos DB
    I need to be able to supply the necessary details and credentials in various different ways
    So that I can connect to the correct storage account while meeting the security requirements of my application

Scenario: Connection string as plain text
    Given CosmosContainerConfiguration of
        """
        {
          "config": {
            "Database": "MyDatabase",
            "Container": "MyContainer",
            "ConnectionStringPlainText": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
          }
        }
        """
    When I validate Cosmos DB storage configuration 'config'
    Then Cosmos DB storage configuration validation succeeds
    And validation determines that the Cosmos DB storage configuration type is 'ConnectionStringAsPlainText'

Scenario: Connection string in key vault
    Given CosmosContainerConfiguration of
        """
        {
          "config": {
            "Database": "MyDatabase",
            "Container": "MyContainer",
            "ConnectionStringInKeyVault": {
              "VaultName": "myvault",
              "SecretName": "secret"
            }
          }
        }
        """
    When I validate Cosmos DB storage configuration 'config'
    Then Cosmos DB storage configuration validation succeeds
    And validation determines that the Cosmos DB storage configuration type is 'ConnectionStringInKeyVault'

Scenario: Account URI with access key as plain text
    Given CosmosContainerConfiguration of
        """
        {
          "config": {
            "Database": "MyDatabase",
            "Container": "MyContainer",
            "AccountUri": "https://example.com/foo",
            "AccessKeyPlainText": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
          }
        }
        """
    When I validate Cosmos DB storage configuration 'config'
    Then Cosmos DB storage configuration validation succeeds
    And validation determines that the Cosmos DB storage configuration type is 'AccountUriAndAccessKeyAsPlainText'

Scenario: Account URI with access key in key vault
    Given CosmosContainerConfiguration of
        """
        {
          "config": {
            "Database": "MyDatabase",
            "Container": "MyContainer",
            "AccountUri": "https://example.com/foo",
            "AccessKeyInKeyVault": {
              "VaultName": "myvault",
              "SecretName": "secret"
            }
          }
        }
        """
    When I validate Cosmos DB storage configuration 'config'
    Then Cosmos DB storage configuration validation succeeds
    And validation determines that the Cosmos DB storage configuration type is 'AccountUriAndAccessKeyInKeyVault'

Scenario: Account URI with client identity
    Given CosmosContainerConfiguration of
        """
        {
          "config": {
            "Database": "MyDatabase",
            "Container": "MyContainer",
            "AccountUri": "https://example.com/foo",
            "ClientIdentity": {
              "IdentitySourceType": "Managed"
            }
          }
        }
        """
    When I validate Cosmos DB storage configuration 'config'
    Then Cosmos DB storage configuration validation succeeds
    And validation determines that the Cosmos DB storage configuration type is 'AccountUriAndClientIdentity'
