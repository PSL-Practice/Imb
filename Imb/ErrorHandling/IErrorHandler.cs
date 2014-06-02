using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imb.ErrorHandling
{
    public interface IErrorHandler
    {
        void LogError(string title, string message, Exception e = null);
    }
}
