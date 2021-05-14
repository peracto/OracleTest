using System;
using System.IO;
using System.Security.Cryptography;

namespace OracleTest { 

    internal static class FileHelpers
    {
        public static bool IsCompressed(string file)
            =>  file.EndsWith(".gz") || file.EndsWith(".brotli") || file.EndsWith(".br");

        public static string GetSha256Digest(string path)
        {
            using var stream = File.OpenRead(path);
            using var m = SHA256.Create();
            var hash = m.ComputeHash(stream);
            stream.Close();
            return Convert.ToBase64String(hash);
        }
    }
}