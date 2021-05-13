using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace OracleTest { 

    internal static class FileHelpers
    {
        public static bool IsCompressed(string file)
            =>  file.EndsWith(".gz") || file.EndsWith(".brotli") || file.EndsWith(".br");

        public static string GetSha256Digest(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var m = SHA256.Create())
            {
                var hash = m.ComputeHash(stream);
                return Convert.ToBase64String(hash);
            }
        }
        
        public static string NormalisePath(string path)
        {
            var p = path.Trim();
            return Path.GetFullPath(
                p.StartsWith('~')
                    ? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), p.Substring(1))
                    : p
            );
        }
    }
    
}