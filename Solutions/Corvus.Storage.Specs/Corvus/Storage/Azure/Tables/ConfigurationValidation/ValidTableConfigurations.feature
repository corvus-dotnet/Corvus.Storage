Feature: ValidTableConfigurations
    As the person responsible for deploying and configuring an application that uses Azure Table Storage
    I need to be able to supply the necessary details and credentials in various different ways
    So that I can connect to the correct storage account while meeting the security requirements of my application

Scenario: Connection string as plain text
    Given TableConfiguration configuration of
        """
        {
          "config": {
            "TableName": "MyTable",
            "ConnectionStringPlainText": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
          }
        }
        """
    When I validate table configuration 'config'
    Then table configuration validation succeeds
    And validation determines that the table configuration type is 'ConnectionStringAsPlainText'

Scenario: Connection string in key vault
    Given TableConfiguration configuration of
        """
        {
          "config": {
            "TableName": "MyTable",
            "ConnectionStringInKeyVault": {
              "VaultName": "myvault",
              "SecretName": "secret"
            }
          }
        }
        """
    When I validate table configuration 'config'
    Then table configuration validation succeeds
    And validation determines that the table configuration type is 'ConnectionStringInKeyVault'

Scenario: Account with access key as plain text
    Given TableConfiguration configuration of
        """
        {
          "config": {
            "TableName": "MyTable",
            "AccountName": "myaccount",
            "AccessKeyPlainText": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
          }
        }
        """
    When I validate table configuration 'config'
    Then table configuration validation succeeds
    And validation determines that the table configuration type is 'AccountNameAndAccessKeyAsPlainText'

Scenario: Account with access key in key vault
    Given TableConfiguration configuration of
        """
        {
          "config": {
            "TableName": "MyTable",
            "AccountName": "myaccount",
            "AccessKeyInKeyVault": {
              "VaultName": "myvault",
              "SecretName": "secret"
            }
          }
        }
        """
    When I validate table configuration 'config'
    Then table configuration validation succeeds
    And validation determines that the table configuration type is 'AccountNameAndAccessKeyInKeyVault'

Scenario: Account with client identity
    Given TableConfiguration configuration of
        """
        {
          "config": {
            "TableName": "MyTable",
            "AccountName": "myaccount",
            "ClientIdentity": {
              "IdentitySourceType": "Managed"
            }
          }
        }
        """
    When I validate table configuration 'config'
    Then table configuration validation succeeds
    And validation determines that the table configuration type is 'AccountNameAndClientIdentity'
