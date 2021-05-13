using System;
using System.IO;
using System.Threading.Tasks;
using Snowflake.FileStream;
using Snowflake.FileStream.Model;
using System.Threading;
using System.IO.Compression;

namespace OracleTest
{
    class CompressPipelineTask : PipelineTaskBase<DataSourceFile, IReflectPipelineTask<DataSourceFile>>
    {
        private readonly SnowflakeCredential _snowflakeCredential;
        private DateTime _nextCredential = DateTime.MinValue;
        private SnowflakePutResponse response;
        private IPutFile putClient;

        public static IPipelineTask<DataSourceFile, IReflectPipelineTask<DataSourceFile>> Create(SnowflakeCredential snowflakeCredential, Action<IReflectPipelineTask<DataSourceFile>> callback)
        {
            return new CompressPipelineTask(snowflakeCredential, callback);
        }

        private CompressPipelineTask(SnowflakeCredential snowflakeCredential, Action<IReflectPipelineTask<DataSourceFile>> callback) : base(callback)
        {
            _snowflakeCredential = snowflakeCredential;
        }

        private async Task RenewCredentials()
        {
            response = await _snowflakeCredential.DoIt();
            _nextCredential = DateTime.Now.AddSeconds(60 * 15);
            if (putClient != null) putClient.Dispose();
            putClient = PutFiles.Create(response);
        }

        public override async Task Execute(DataSourceFile dataSourceFile)
        {
            if (DateTime.Now > _nextCredential)
                await RenewCredentials();

            var file = dataSourceFile.Filename;

            var inputFile = response.SourceCompression != "none" && response.AutoCompress && !FileHelpers.IsCompressed(file)
                          ? await CompressFile(file, response.SourceCompression)
                          : file;

            var encryptFile = await EncryptFile(inputFile);

            Output(new OutputFile(dataSourceFile, putClient, file, inputFile, encryptFile));
        }

        private static async Task<string> CompressFile(string filename, string compressionType)
        {
            var outfile = Path.GetTempFileName();

            using var originalFileStream = File.OpenRead(filename);
            using var compressedFileStream = File.OpenWrite(outfile);

            using Stream stream = compressionType switch
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
            return putClient.Crypto.EncryptFile(
                filename,
                encryptFile
             ).ContinueWith(e => encryptFile);
        }


        private class OutputFile : IReflectPipelineTask<DataSourceFile>
        {
            private string OriginalFilename { get; }
            private string IntermediateFilename { get; }
            private string TransferFilename { get; }

            public DataSourceFile DataSourceFile {get;}

            public IPutFile PutFile { get; }

            public OutputFile(DataSourceFile file, IPutFile putFile, string originalFilename, string intermediateFilename, string transferFilename)
            {
                OriginalFilename = originalFilename;
                IntermediateFilename = intermediateFilename;
                TransferFilename = transferFilename;
                DataSourceFile = file;
                PutFile = putFile;
            }

            public Task Execute(CancellationToken token)
            {
                return PutFile.Put(
                    TransferFilename,
                    Path.GetFileName(OriginalFilename),
                    FileHelpers.GetSha256Digest(IntermediateFilename),
                    token
                    );
            }

            public DataSourceFile GetReflect()
            {
                return DataSourceFile;
            }

            public override string ToString()
            {
                return OriginalFilename;
            }
        }
    }
}
