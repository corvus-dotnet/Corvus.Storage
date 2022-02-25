Feature: CloudTable
    As the person responsible for deploying and configuring an application that uses Azure Tables
    I need to be able to supply the necessary details and credentials in various different ways
    So that I can connect to the correct storage account while meeting the security requirements of my application

Scenario: Connection string in configuration
    Given TableConfiguration configuration of
        """
        {
          "config": {
            "TableName": "MyTable",
            "ConnectionStringPlainText": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
          }
        }
        """
    When I get a table client for 'config' as 'c1'
    And I get a table client for 'config' as 'c2'
    Then the storage client endpoint in table client 'c1' should specify account 'devstoreaccount1' and table 'MyTable'
    # Would like to test that the AccountKey is also present, but there isn't a straightforward way to do that.
    And the TableClient for tables 'c1' and 'c2' should be the same instance

Scenario: Account name and managed identity
    Given TableConfiguration configuration of
        """
        {
          "config": {
            "TableName": "MyTable",
            "AccountName": "myaccount",
            "ClientIdentity": {
              "IdentitySourceType": "Managed"
            },
          }
        }
        """
    When I get a table client for 'config' as 'c1'
    And I get a table client for 'config' as 'c2'
    Then the storage client endpoint in table client 'c1' should specify account 'myaccount' and table 'MyTable'
    And the TableClient for tables 'c1' and 'c2' should be the same instance
    And the TableConfiguration.ClientIdentity from 'config' should have been passed to the token credential source
    # Would like to test that the TokenCredential returned by the token credential source was used, but there isn't a straightforward way to do that.
