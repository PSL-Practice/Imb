using System;
using System.Text;
using Imb.ErrorHandling;

namespace TestImb.Mocks
{
    public class MockErrorHandler : IErrorHandler
    {
        private StringBuilder _sb = new StringBuilder();

        public string History { get { return _sb.ToString(); } }

        public void LogError(string title, string message, Exception e = null)
        {
            if (_sb.Length > 0) _sb.AppendLine();

            _sb.AppendLine(title);
            _sb.AppendLine(message);
            if (e != null)
                _sb.AppendLine(e.ToString());
        }
    }
}