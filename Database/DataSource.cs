using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace OracleTest.Database
{
    class DataSource
    {
        public readonly string SchemaName;
        public readonly string TableName;
        private int _tokenCount = 0;
        private readonly DataSourceController _controller;

        public void AddRef()
        {
            Interlocked.Increment(ref _tokenCount);
        }
        public void ReleaseRef()
        {
            if (Interlocked.Decrement(ref _tokenCount) == 0)
                _controller.Trigger(this);
        }
        
        public DataSource(DataSourceController controller, string schemaName, string tableName)
        {
            SchemaName = schemaName;
            TableName = tableName;
            MaxFileSize = 30 * 1024 * 1024;
            _controller = controller;
        }

        public IEnumerable<string> CreateQueries(int size)
        {
            for (var i = 0; i < size; i++)
                yield return $"select * from {SchemaName}.{TableName} where mod(booking_id,{size}) = {i}";
        }
        public string FullName => SchemaName + "." + TableName;
        public int MaxFileSize { get; }
    }
}
