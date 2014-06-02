using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imb.Utils
{
    public class PathAdjuster
    {
        private readonly string[] _oldRoot;
        private readonly string[] _newRoot;
        private static readonly string[] Empty = { };

        public PathAdjuster(string[] oldRoot, string[] newRoot)
        {
            _oldRoot = oldRoot ?? Empty;
            _newRoot = newRoot ?? Empty;
        }

        public string[] AdjustPath(string[] path)
        {
            var strippedOfOld = path == null ? Empty : path.Skip(_oldRoot.Length);
            return _newRoot.Concat(strippedOfOld).ToArray();
        }
    }
}
