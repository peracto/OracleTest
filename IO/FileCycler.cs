using System;
using System.IO;

namespace OracleTest
{
    class FileCycler : IDisposable
    {
        private int _index;
        private string _currentFile;
        private Action<string> _cb;
        private readonly Func<int, string> _template;

        public FileCycler(Func<int, string> template, Action<string> cb)
        {
            _template = template;
            _cb = cb;
        }

        public Stream CreateStream()
        {
            Close();
            _currentFile = _template(_index++);
            return File.Create(_currentFile);
        }

        public void Close()
        {
            if (_currentFile == null) return;
            _cb(_currentFile);
            _currentFile = null;
        }
        public string CurrentFileName => _currentFile;

        public void Dispose()
        {
            Close();
        }
    }
}
