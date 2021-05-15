using System.Collections.Generic;

namespace OracleTest.Database
{
    class DataSource
    {
        public readonly string SchemaName;
        public readonly string TableName;

        public DataSource(string schemaName, string tableName)
        {
            SchemaName = schemaName;
            TableName = tableName;
            MaxFileSize = 30 * 1024 * 1024;
        }

        public string FullName => SchemaName + "." + TableName;
        public string LifetimeKey => SchemaName + "." + TableName;
        public int MaxFileSize { get; }
    }

}
