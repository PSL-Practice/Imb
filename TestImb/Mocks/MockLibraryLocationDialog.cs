using System.Text;
using System.Windows;
using Imb.ViewModels;

namespace TestImb.Mocks
{
    internal class MockLibraryLocationDialog : ILibraryLocationDialog
    {
        public string PathToReturn { get; set; }

        private StringBuilder _sb = new StringBuilder();

        public string History { get { return _sb.ToString(); }}

        public string GetNewLibraryLocation(string libraryPath, Window window = null)
        {
            _sb.AppendLine("Requested new library location.");
            return PathToReturn;
        }

        public string GetExistingLibraryLocation(string libraryPath, Window window = null)
        {
            _sb.AppendLine("Requested existing library location.");
            return PathToReturn;
        }

        public string GetFileLocation(Window window = null)
        {
            _sb.AppendLine("Requested file location.");
            return PathToReturn;
        }
    }
}