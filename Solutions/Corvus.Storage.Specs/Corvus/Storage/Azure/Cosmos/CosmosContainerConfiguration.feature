Feature: CosmosContainerConfiguration
    As the person responsible for deploying and configuring an application that uses Cosmos DB via SQL
    I need to be able to supply the necessary details and credentials in various different ways
    So that I can connect to the correct database while meeting the security requirements of my application


# Scenarios to check
#
# Just the connection string (with embedded credentials):
#   Plaintext ConnectionString
#   Connection string in key vault
#
# Account name with secret
#
#
# Account name, no secret, Azure AD auth
#  

# This should typically only be used for local dev, ideally using a not-really-secret. This example
# uses the well-known key that the Cosmos DB emulator recognizes.
Scenario: Connection string in configuration
    Given CosmosContainerConfiguration of
        """
        {
          "config": {
            "ConnectionStringPlainText": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            "Database": "MyDb",
            "Container": "MyContainer"
          }
        }
        """
    When I get a Cosmos DB container for 'config' as 'c1'
    And I get a Cosmos DB container for 'config' as 'c2'
    Then the CosmosClient.Endpoint in 'c1' should be 'https://localhost:8081/'
    # Would like to test that the AccountKey is also present, but there isn't a straightforward way to do that.
    # The Cosmos client libraries don't seem to make the key available (which is probably sensible for most
    # scenarios). We could test this by setting up an HTTP listener to receive the request, and capturing
    # the signature it sends, but we'd then need to re-implement the signature validation logic ourselves.
    # There might be more value in creating integration tests for all the scenarios we care about instead.
    And the Cosmos Database in 'c1' is 'MyDb'
    And the Cosmos Container in 'c1' is 'MyContainer'
    And the CosmosClient for containers 'c1' and 'c2' should be the same instance

# Cosmos DB client SDK guidelines recommend that you share a single CosmosClient across your whole
# app when talking to any particular Cosmos DB
Scenario: Connection string in configuration with same database, different containers
    Given CosmosContainerConfiguration of
        """
        {
          "config1": {
            "ConnectionStringPlainText": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            "Database": "MyDb",
            "Container": "MyContainer1"
          },
          "config2": {
            "ConnectionStringPlainText": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            "Database": "MyDb",
            "Container": "MyContainer2"
          }
        }
        """
    When I get a Cosmos DB container for 'config1' as 'c1'
    And I get a Cosmos DB container for 'config2' as 'c2'
    Then the CosmosClient.Endpoint in 'c1' should be 'https://localhost:8081/'
    # Would like to test that the AccountKey is also present, but there isn't a straightforward way to do that.
    # The Cosmos client libraries don't seem to make the key available (which is probably sensible for most
    # scenarios). We could test this by setting up an HTTP listener to receive the request, and capturing
    # the signature it sends, but we'd then need to re-implement the signature validation logic ourselves.
    # There might be more value in creating integration tests for all the scenarios we care about instead.
    And the Cosmos Database in 'c1' is 'MyDb'
    And the Cosmos Database in 'c2' is 'MyDb'
    And the Cosmos Container in 'c1' is 'MyContainer1'
    And the Cosmos Container in 'c2' is 'MyContainer2'
    And the CosmosClient for containers 'c1' and 'c2' should be the same instance
