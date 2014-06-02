using System;

namespace Imb.Data
{
    public class NotFoundInLibraryException : Exception
    {
        public Guid Id { get; private set; }

        public NotFoundInLibraryException(Guid id) : base(string.Format("Id {0} not found in library.", id))
        {
            Id = id;
        }
    }
}