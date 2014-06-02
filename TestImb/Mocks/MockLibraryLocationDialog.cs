using System.Text;
using Imb.ViewModels;

namespace TestImb.Mocks
{
    internal class MockLibraryLocationDialog : ILibraryLocationDialog
    {
        public string PathToReturn { get; set; }

        private StringBuilder _sb = new StringBuilder();

        public string History { get { return _sb.ToString(); }}

        public string GetNewLibraryLocation(string libraryPath)
        {
            _sb.AppendLine("Requested new library location.");
            return PathToReturn;
        }

        public string GetExistingLibraryLocation(string libraryPath)
        {
            _sb.AppendLine("Requested existing library location.");
            return PathToReturn;
        }

        public string GetFileLocation()
        {
            _sb.AppendLine("Requested file location.");
            return PathToReturn;
        }
    }
}