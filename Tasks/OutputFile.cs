using Snowflake.FileStream;

namespace OracleTest.Tasks
{
    public class OutputFile
    {
        public string BucketKey { get; }
        public string Digest { get; }
        public string Filename { get; }
        public string LifetimeKey { get; }

        public IPutFile PutFile { get; }

        public OutputFile(IPutFile putFile,string lifetimeKey, string bucketKey, string digest, string filename)
        {
            PutFile = putFile;
            BucketKey = bucketKey;
            Digest = digest;
            Filename = filename;
            LifetimeKey = lifetimeKey;
        }
    }

}
