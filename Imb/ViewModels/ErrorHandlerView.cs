using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imb.ErrorHandling;

namespace Imb.ViewModels
{
    public class ErrorHandlerView : IErrorHandlerView, IErrorHandler
    {
        public IErrorHandler ErrorHandler { get { return this; } }
        public ObservableCollection<string> Errors { get; private set; }

        public ErrorHandlerView()
        {
            Errors = new ObservableCollection<string>();
        }
        public void LogError(string title, string message, Exception e = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine(message);
            if (e != null)
               sb.AppendLine(e.ToString());
            Errors.Add(sb.ToString());
        }
    }

    public interface IErrorHandlerView
    {
        IErrorHandler ErrorHandler { get; }
        ObservableCollection<string> Errors { get; } 
    }
}
