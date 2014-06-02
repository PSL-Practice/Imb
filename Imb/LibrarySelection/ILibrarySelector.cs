using Imb.Data.View;
using Imb.Utils;

namespace Imb.LibrarySelection
{
    public interface ILibrarySelector
    {
        ILibraryView CreateLibrary(string path);
        ILibraryView OpenLibrary(string path);
        void CloseLibrary(ILibraryView library);
    }
}