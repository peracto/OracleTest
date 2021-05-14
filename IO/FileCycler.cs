using System;
using System.IO;

namespace OracleTest.IO
{
    internal class FileCycler 
    {
        private int _index;
        private readonly Func<int, string> _template;

        public FileCycler(Func<int, string> template)
        {
            _template = template;
        }

        public Stream CreateStream()
        {
            CurrentFileName = _template(_index++);
            return File.Create(CurrentFileName);
        }

        public string CurrentFileName { get; private set; }
    }
}
