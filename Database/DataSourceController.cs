using System;

namespace OracleTest.Database
{
    class DataSourceController
    {
        public void Trigger(DataSource obj)
        {
            Console.WriteLine($@"Triggered {obj.FullName}");
        }
    }
}