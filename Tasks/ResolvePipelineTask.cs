using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace OracleTest
{
    class ResolvePipelineTask : PipelineTaskBase<DataSourceFile, DataSourceFile>
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
            Console.WriteLine($"Resolved:::{slice.Filename}");
            return Task.FromResult(1);
        }
    }
}
