using System.IO;

namespace OracleTest.Model
{
    internal class PutFileItem : IPutFileItem
    {
        public PutFileItem(string filename, string key)
        {
            Filename = filename;
            Key = key;
        }
        public PutFileItem(string filename)
        {
            Filename = filename;
            Key = Path.GetFileName(filename);
        }

        public string Filename { get; }
        public string Key { get; }
    }
}