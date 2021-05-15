using System.Threading.Tasks;

namespace Bourne.BatchLoader.Pipeline
{
    public interface IPipelineTask<in TIn, TOut>
    {
        Task<TOut> Execute(TIn data);
        Task Start();
        Task End();
    }
}
