Feature: InvalidSqlDatabaseConfigurations
    As the person responsible for deploying and configuring an application that uses SQL Server or Azure SQL databases
    I need to configuration that supplies the necessary details and credentials in various different ways to be accepted as valid
    So that I can connect to the correct storage account while meeting the security requirements of my application

Scenario Outline: Null settings
    When I validate a null SqlDatabaseConfiguration
    Then the SqlDatabaseConfiguration should be reported as invalid

Scenario Outline: No settings
    Given a SqlDatabaseConfiguration
    When I validate the SqlDatabaseConfiguration
    Then the SqlDatabaseConfiguration should be reported as invalid

Scenario Outline: No connection string
    Given a SqlDatabaseConfiguration
    And SqlDatabaseConfiguration.ClientIdentity set to use '<clientIdentityType>'
    When I validate the SqlDatabaseConfiguration
    Then the SqlDatabaseConfiguration should be reported as invalid

    Examples:
        | clientIdentityType |
        | None               |
        | Managed            |

Scenario Outline: Connection string in both plain text and key vault
    Given a SqlDatabaseConfiguration
    And a SqlDatabaseConfiguration.ConnectionStringPlainText of 'Server=(localdb)\\mssqllocaldb;Trusted_Connection=True'
    And SqlDatabaseConfiguration.ConnectionStringInKeyVault set to use vault 'myvault' and secret 'mysecret'
    And SqlDatabaseConfiguration.ClientIdentity set to use '<clientIdentityType>'
    When I validate the SqlDatabaseConfiguration
    Then the SqlDatabaseConfiguration should be reported as invalid

    Examples:
        | clientIdentityType |
        | None               |
        | Managed            |
