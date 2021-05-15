using System;
using System.Threading;
using System.Threading.Tasks;
using Bourne.Common.Pipeline;

namespace OracleTest.Tasks
{
    internal class UploadPipelineTask : PipelineTaskBase<OutputFile, OutputFile>
    {
        public UploadPipelineTask(Action<OutputFile> callback) : base(callback)
        {
        }

        public override async Task Execute(OutputFile file)
        {
            await file.PutFile.Put(
               file.Filename,
               file.BucketKey,
               file.Digest,
               CancellationToken.None
           );
            Output(file);
        }
    }
}
