using System.Collections.Generic;

namespace OracleTest
{
    class DataSource
    {
        public readonly string SchemaName;
        public readonly string TableName;
        private int _maxBatchSize;

        public DataSource(string schemaName, string tableName)
        {
            SchemaName = schemaName;
            TableName = tableName;
            _maxBatchSize = 30 * 1024 * 1024;
        }

        public IEnumerable<string> CreateQueries(int size)
        {
            for (var i = 0; i < size; i++)
                yield return $"select * from {SchemaName}.{TableName} where mod(booking_id,{size}) = {i}";
        }
        public string FullName => SchemaName + "." + TableName;
        public int MaxFileSize => _maxBatchSize;

    }
}
