using System;
using System.Threading.Tasks;
using Bourne.Common.Pipeline;
using OracleTest.Database;
using OracleTest.Model;

namespace OracleTest.Tasks
{
    internal class ResolvePipelineTask : PipelineTaskBase<DataSource, DataSource>
    {
        public ResolvePipelineTask(Action<DataSource> callback) : base(callback)
        {
        }

        public override Task Execute(DataSource slice)
        {
            Console.WriteLine($@"Resolved:::{slice.FullName}");
            return Task.FromResult(1);
        }
    }
}
