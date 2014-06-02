using System.Text;
using System.Windows.Threading;
using Imb.Data.View;
using Imb.EventAggregation;
using Imb.LibrarySelection;
using Imb.Utils;

namespace TestImb.Mocks
{
    public class MockLibrarySelector : ILibrarySelector
    {
        private readonly MockLibraryOperations _libraryOperations;
        private Dispatcher _dispatcher;
        private StringBuilder _sb = new StringBuilder();
        private MockErrorHandler _errorHandler;
        private MockFileValidator _fileValidator;
        private UnitTestEventAggregator _eventAggregator;

        public MockLibrarySelector(Dispatcher dispatcher, MockLibraryOperations libraryOperations, MockErrorHandler errorHandler, MockFileValidator fileValidator, UnitTestEventAggregator eventAggregator)
        {
            _dispatcher = dispatcher;
            _errorHandler = errorHandler;
            _fileValidator = fileValidator;
            _eventAggregator = eventAggregator;
            _libraryOperations = libraryOperations ?? new MockLibraryOperations(_eventAggregator);
        }

        public string History { get { return _sb.ToString(); } }

        public ILibraryView CreateLibrary(string path)
        {
            _sb.AppendLine(string.Format("Create library in location \"{0}\".", path));
            return new MockLibraryView(_dispatcher, _libraryOperations, _errorHandler, _fileValidator, path, _eventAggregator);
        }

        public ILibraryView OpenLibrary(string path)
        {
            _sb.AppendLine(string.Format("Open library in location \"{0}\".", path));
            return new MockLibraryView(_dispatcher, _libraryOperations, _errorHandler, _fileValidator, path, _eventAggregator);
        }

        public void CloseLibrary(ILibraryView library)
        {
            _sb.AppendLine(string.Format("Close library in location \"{0}\".", ((MockLibraryView)library).Path));
        }
    }
}