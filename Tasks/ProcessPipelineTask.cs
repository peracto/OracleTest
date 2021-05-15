using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Bourne.BatchLoader.IO;
using Bourne.BatchLoader.Model;
using Bourne.BatchLoader.Pipeline;
using Snowflake.FileStream;

namespace Bourne.BatchLoader.Tasks
{
    internal class ProcessPipelineTask : PipelineTaskBase<DataSourceFile, UploadItem>
    {
        private readonly StorageManager _storage;
        private IPutFile _storageClient;

        public ProcessPipelineTask(StorageManager storage) 
        {
            _storage = storage;
        }

        public override async Task<UploadItem> Execute(DataSourceFile dataSourceFile)
        {
            if (_storageClient == null || _storageClient.IsExpired)
                _storageClient = await _storage.Renew();

            var file = dataSourceFile.Filename;

            var inputFile = _storageClient.IsCompressed
                ? await CompressFile(file, _storageClient.SourceCompression)
                : file;

            var encryptFile = await _storageClient.EncryptFile(
                inputFile,
                Path.GetTempFileName()
            );

            var digest = FileHelpers.GetSha256Digest(inputFile);

            File.Delete(file);
            if (file != inputFile)
                File.Delete(inputFile);
            
            return new UploadItem(
                storage: _storageClient,
                lifetimeKey: dataSourceFile.LifetimeKey,
                bucketKey: Path.GetFileName(file),
                digest: digest,
                filename: encryptFile
            );
        }

        private static async Task<string> CompressFile(string filename, string compressionType)
        {
            var outfile = Path.GetTempFileName();

            await using var originalFileStream = File.OpenRead(filename);
            await using var compressedFileStream = File.OpenWrite(outfile);
            await using Stream stream = compressionType switch
            {
                "brotli" => new BrotliStream(compressedFileStream, CompressionMode.Compress),
                _ => new GZipStream(compressedFileStream, CompressionMode.Compress),
            };

            await originalFileStream.CopyToAsync(stream);
            return outfile;
        }
    }
}
