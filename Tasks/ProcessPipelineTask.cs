using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Bourne.Common.Pipeline;
using OracleTest.IO;
using OracleTest.Model;
using Snowflake.FileStream;
using Snowflake.FileStream.Model;

namespace OracleTest.Tasks
{
    internal class ProcessPipelineTask : PipelineTaskBase<DataSourceFile, IReflectPipelineTask<DataSourceFile>>
    {
        private readonly SnowflakeCredential _snowflakeCredential;
        private DateTime _nextCredential = DateTime.MinValue;
        private SnowflakePutResponse _response;
        private IPutFile _putClient;

        public static IPipelineTask<DataSourceFile, IReflectPipelineTask<DataSourceFile>> Create(SnowflakeCredential snowflakeCredential, Action<IReflectPipelineTask<DataSourceFile>> callback)
        {
            return new ProcessPipelineTask(snowflakeCredential, callback);
        }

        private ProcessPipelineTask(SnowflakeCredential snowflakeCredential, Action<IReflectPipelineTask<DataSourceFile>> callback) : base(callback)
        {
            _snowflakeCredential = snowflakeCredential;
        }

        private async Task RenewCredentials()
        {
            _response = await _snowflakeCredential.DoIt();
            _nextCredential = DateTime.Now.AddSeconds(60 * 15);
            _putClient?.Dispose();
            _putClient = PutFiles.Create(_response);
        }

        public override async Task Execute(DataSourceFile dataSourceFile)
        {
            if (DateTime.Now > _nextCredential)
                await RenewCredentials();

            var file = dataSourceFile.Filename;

            var inputFile = _response.SourceCompression != "none" && _response.AutoCompress && !FileHelpers.IsCompressed(file)
                          ? await CompressFile(file, _response.SourceCompression)
                          : file;

            var encryptFile = await EncryptFile(inputFile);

            Output(new OutputFile(dataSourceFile, _putClient, file, inputFile, encryptFile));
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

        private Task<string> EncryptFile(string filename)
        {
            var encryptFile = Path.GetTempFileName();
            return _putClient.Crypto.EncryptFile(
                filename,
                encryptFile
             ).ContinueWith(e => encryptFile);
        }


        private class OutputFile : IReflectPipelineTask<DataSourceFile>
        {
            private string OriginalFilename { get; }
            private string IntermediateFilename { get; }
            private string TransferFilename { get; }

            private DataSourceFile DataSourceFile {get;}

            private IPutFile PutFile { get; }

            public OutputFile(DataSourceFile file, IPutFile putFile, string originalFilename, string intermediateFilename, string transferFilename)
            {
                OriginalFilename = originalFilename;
                IntermediateFilename = intermediateFilename;
                TransferFilename = transferFilename;
                DataSourceFile = file;
                PutFile = putFile;
            }

            public Task Execute(CancellationToken token) =>
                PutFile.Put(
                    TransferFilename,
                    Path.GetFileName(OriginalFilename),
                    FileHelpers.GetSha256Digest(IntermediateFilename),
                    token
                );

            public DataSourceFile GetReflect() => 
                DataSourceFile;

            public override string ToString() => 
                OriginalFilename;
        }
    }
}
