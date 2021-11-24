Feature: BlobContainerConfiguration
    As the person responsible for deploying and configuring an application that use Azure Blob Storage
    I need to be able to supply the necessary details and credentials in various different ways
    So that I can connect to the correct storage account while meeting the security requirements of my application

#Scenario: Add two numbers
#    Given the first number is 50
#    And the second number is 70
#    When the two numbers are added
#    Then the result should be 120

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
    Given configuration of
        """
        {
          "SomeStorageConfig": {
            "ConnectionStringPlainText": "DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=mykey"
          }
        }
        """


# Make support for legacy config a configurable thing, because it looks like entirely missing config ends
# up looking like a valid configuration, because it defaults to handling a null or empty AccountName as meaning
# "use local storage".