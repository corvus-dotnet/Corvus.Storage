Feature: ValidBlobContainerConfigurations
	As the person responsible for deploying and configuring an application that use Azure Blob Storage
    I need to be able to supply the necessary details and credentials in various different ways
    So that I can connect to the correct storage account while meeting the security requirements of my application

Scenario: Connection string as plain text
    Given BlobContainerConfiguration configuration of
        """
        {
          "config": {
            "Container": "MyContainer",
            "ConnectionStringPlainText": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
          }
        }
        """
    When I validate blob storage configuration 'config'
    Then blob storage configuration validation succeeds
    And validation determines that the blob storage configuration type is 'ConnectionStringAsPlainText'

Scenario: Connection string in key vault
    Given BlobContainerConfiguration configuration of
        """
        {
          "config": {
            "Container": "MyContainer",
            "ConnectionStringInKeyVault": {
              "VaultName": "myvault",
              "SecretName": "secret"
            }
          }
        }
        """
    When I validate blob storage configuration 'config'
    Then blob storage configuration validation succeeds
    And validation determines that the blob storage configuration type is 'ConnectionStringInKeyVault'

Scenario: Account with access key as plain text
    Given BlobContainerConfiguration configuration of
        """
        {
          "config": {
            "Container": "MyContainer",
            "AccountName": "myaccount",
            "AccessKeyPlainText": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
          }
        }
        """
    When I validate blob storage configuration 'config'
    Then blob storage configuration validation succeeds
    And validation determines that the blob storage configuration type is 'AccountNameAndAccessKeyAsPlainText'

Scenario: Account with access key in key vault
    Given BlobContainerConfiguration configuration of
        """
        {
          "config": {
            "Container": "MyContainer",
            "AccountName": "myaccount",
            "AccessKeyInKeyVault": {
              "VaultName": "myvault",
              "SecretName": "secret"
            }
          }
        }
        """
    When I validate blob storage configuration 'config'
    Then blob storage configuration validation succeeds
    And validation determines that the blob storage configuration type is 'AccountNameAndAccessKeyInKeyVault'

Scenario: Account with client identity
    Given BlobContainerConfiguration configuration of
        """
        {
          "config": {
            "Container": "MyContainer",
            "AccountName": "myaccount",
            "ClientIdentity": {
              "IdentitySourceType": "Managed"
            }
          }
        }
        """
    When I validate blob storage configuration 'config'
    Then blob storage configuration validation succeeds
    And validation determines that the blob storage configuration type is 'AccountNameAndClientIdentity'
