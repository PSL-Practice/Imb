using System.Windows;

namespace Imb.ViewModels
{
    public interface ILibraryLocationDialog
    {
        string GetNewLibraryLocation(string defaultPath, Window window = null);
        string GetExistingLibraryLocation(string defaultPath, Window window = null);
        string GetFileLocation( Window window = null);
    }
}