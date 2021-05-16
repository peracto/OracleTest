namespace Bourne.BatchLoader.Model
{
    internal class DataSource
    {
        public DataSource(string datasourceName, string splitKey, int splitSize, int batchFileSize)
        {
            FullName = datasourceName;
            SplitKey = splitKey;
            SplitSize = splitSize;
            BatchFileSize = batchFileSize;
        }

        public string SplitKey { get; }
        public int SplitSize { get; }
        public int BatchFileSize { get; }
        public string FullName { get; }
        public string LifetimeKey => FullName;
    }
}
