using System;

namespace Imb.Data
{
    public class RootNotEmptyException : Exception
    {
        public RootNotEmptyException(string root) : base(string.Format("The root directory \"{0}\" already exists.", root))
        {
        }
    }

    public class RootNotFoundException : Exception
    {
        public RootNotFoundException(string root)
            : base(string.Format("The root directory \"{0}\" does not exist.", root))
        {
        }
    }

    public class RootNotDirectoryException : Exception
    {
        public RootNotDirectoryException(string root)
            : base(string.Format("The root \"{0}\" is not a directory.", root))
        {
        }
    }
}