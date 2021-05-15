namespace Bourne.BatchLoader.Model
{
    internal class DataSourceFile 
    {
        public int RowCount { get; }
        public string LifetimeKey { get; }
        public string Filename { get; }
        public DataSourceFile(string filename, string lifetimeKey, int rowCount)
        {
            RowCount = rowCount;
            LifetimeKey = lifetimeKey;
            Filename = filename;
        }
    }
}
