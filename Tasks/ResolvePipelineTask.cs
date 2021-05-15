using System;
using System.Threading.Tasks;
using Bourne.BatchLoader.Model;
using Bourne.BatchLoader.Pipeline;

namespace Bourne.BatchLoader.Tasks
{
    internal class ResolvePipelineTask : PipelineTaskBase<DataSource, int>
    {
        public override Task<int> Execute(DataSource slice)
        {
            Console.WriteLine($@"Resolved:::{slice.LifetimeKey}");
            return Task.FromResult(1);
        }
    }
}
