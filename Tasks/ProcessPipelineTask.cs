using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Bourne.Common.Pipeline;
using OracleTest.IO;
using OracleTest.Model;
using Snowflake.FileStream;

namespace OracleTest.Tasks
{
    internal class ProcessPipelineTask : PipelineTaskBase<DataSourceFile, OutputFile>
    {
        private readonly SnowflakeCredentialManager _snowflakeCredential;
        private IPutFile _putClient = null;

        public ProcessPipelineTask(SnowflakeCredentialManager snowflakeCredential, Action<OutputFile> callback) : base(callback)
        {
            _snowflakeCredential = snowflakeCredential;
        }

        public override async Task Execute(DataSourceFile dataSourceFile)
        {
            if (_putClient == null || _putClient.IsExpired)
                _putClient = await _snowflakeCredential.Renew();

            var file = dataSourceFile.Filename;

            var inputFile = _putClient.IsCompressed
                          ? await CompressFile(file, _putClient.SourceCompression)
                          : file;

            var encryptFile = await _putClient.EncryptFile(
                inputFile,
                Path.GetTempFileName()
            );

            var digest = FileHelpers.GetSha256Digest(inputFile);

            Output(new OutputFile(
                putFile: _putClient,
                lifetimeKey: dataSourceFile.LifetimeKey,
                bucketKey: Path.GetFileName(file),
                digest: digest,
                filename: encryptFile
            ));
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
