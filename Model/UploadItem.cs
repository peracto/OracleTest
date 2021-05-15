using Snowflake.FileStream;

namespace Bourne.BatchLoader.Model
{
    internal class UploadItem
    {
        public string BucketKey { get; }
        public string Digest { get; }
        public string Filename { get; }
        public string LifetimeKey { get; }
        public IPutFile Storage { get; }

        public UploadItem(IPutFile storage,string lifetimeKey, string bucketKey, string digest, string filename)
        {
            Storage = storage;
            BucketKey = bucketKey;
            Digest = digest;
            Filename = filename;
            LifetimeKey = lifetimeKey;
        }
    }
}
