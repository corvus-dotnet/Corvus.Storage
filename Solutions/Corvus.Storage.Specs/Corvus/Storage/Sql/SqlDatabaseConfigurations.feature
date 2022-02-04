Feature: SqlDatabaseConfigurations
    As the person responsible for deploying and configuring an application that use Azure SQL or SQL Server
    I need to be able to supply the necessary details and credentials in various different ways
    So that I can connect to the correct database while meeting the security requirements of my application

Scenario: Connection string in configuration
    Given SqlDatabaseConfiguration of
        """
        {
          "config": {
            "ConnectionStringPlainText": "Server=(localdb)\\\\mssqllocaldb;Initial Catalog=mydb;Trusted_Connection=True"
          }
        }
        """
    When I get a SqlConnection for 'config' as 'c1'
    And I get a SqlConnection for 'config' as 'c2'
    Then the SqlConnection.ConnectionString in 'c1' should be 'Server=(localdb)\\mssqllocaldb;Initial Catalog=mydb;Trusted_Connection=True'
    Then the SqlConnection.ConnectionString in 'c2' should be 'Server=(localdb)\\mssqllocaldb;Initial Catalog=mydb;Trusted_Connection=True'
    And the SqlConnections named 'c1' and 'c2' should be different instances
