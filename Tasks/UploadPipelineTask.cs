using System;
using System.Threading;
using System.Threading.Tasks;
using Bourne.BatchLoader.Model;
using Bourne.BatchLoader.Pipeline;

namespace Bourne.BatchLoader.Tasks
{
    internal class UploadPipelineTask : PipelineTaskBase<UploadItem, UploadItem>
    {
        public override async Task<UploadItem> Execute(UploadItem file)
        {
            await file.Storage.Put(
                file.Filename,
                file.BucketKey,
                file.Digest,
                CancellationToken.None
            );
            return file;
        }
    }
}
