namespace sql_server_integration_tests.Infrastructure
{
    [CollectionDefinition(Name)]
    public sealed class SqlServerTestCollection : ICollectionFixture<SqlServerDatabaseFixture>
    {
        public const string Name = "SqlServer";
    }
}
