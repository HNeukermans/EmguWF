using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EmguWF.Extensions
{
    public sealed class OutputWriter : TextWriter
    {
        private readonly IList<string> _log;

        public OutputWriter(IList<string> log)
        {
            _log = log;
        }

        public override void Write(string line)
        {
            _log.Add(line);
        }

        public override void WriteLine(string line)
        {
            _log.Add(line);
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}
