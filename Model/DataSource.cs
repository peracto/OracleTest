namespace Bourne.BatchLoader.Model
{
    internal class DataSource
    {
        public DataSource(string datasourceName)
        {
            FullName = datasourceName;
        }

        public string FullName { get; }

        public string LifetimeKey => FullName;
    }
}
