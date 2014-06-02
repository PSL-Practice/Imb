namespace Imb.ViewModels
{
    public interface ILibraryLocationDialog
    {
        string GetNewLibraryLocation(string defaultPath);
        string GetExistingLibraryLocation(string defaultPath);
        string GetFileLocation();
    }
}