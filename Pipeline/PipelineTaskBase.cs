using System.Threading.Tasks;

namespace Bourne.BatchLoader.Pipeline
{
    public abstract class PipelineTaskBase<TIn, TOut> : IPipelineTask<TIn, TOut>
    {
        public virtual Task Start()
            => Task.CompletedTask;

        public virtual Task End() 
            => Task.CompletedTask;

        public abstract Task<TOut> Execute(TIn data);
    }
}
