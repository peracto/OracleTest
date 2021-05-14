using System;
using System.Threading.Tasks;
using Bourne.Common.Pipeline;
using OracleTest.Model;

namespace OracleTest.Tasks
{
    internal class ResolvePipelineTask : PipelineTaskBase<DataSourceFile, DataSourceFile>
    {
        public static IPipelineTask<DataSourceFile, DataSourceFile> Create(
            Action<DataSourceFile> callback
            )
        {
            return new ResolvePipelineTask(callback);
        }

        private ResolvePipelineTask(Action<DataSourceFile> callback) : base(callback)
        {
        }

        public override Task Execute(DataSourceFile slice)
        {
            Console.WriteLine($@"Resolved:::{slice.Filename}");
            slice.DataSource.ReleaseRef();
            return Task.FromResult(1);
        }
    }
}
