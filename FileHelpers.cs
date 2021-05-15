using System;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Bourne.BatchLoader {

    internal static class FileHelpers
    {
        public static string GetSha256Digest(string path)
        {
            using var stream = File.OpenRead(path);
            using var m = SHA256.Create();
            var hash = m.ComputeHash(stream);
            stream.Close();
            return Convert.ToBase64String(hash);
        }

        public static T ReadJson<T>(string name)
            => JsonConvert.DeserializeObject<T>(File.ReadAllText(name));
    }
}