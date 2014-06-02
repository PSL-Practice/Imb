using System.Collections.ObjectModel;
using Imb.ErrorHandling;
using Imb.ViewModels;

namespace TestImb.Mocks
{
    public class MockErrorHandlerView : IErrorHandlerView
    {
        public MockErrorHandlerView()
        {
            MockErrorHandler = new MockErrorHandler();
            Errors = new ObservableCollection<string>();
        }

        public IErrorHandler ErrorHandler { get { return MockErrorHandler; } }
        public MockErrorHandler MockErrorHandler { get; private set; }
        public ObservableCollection<string> Errors { get; private set; }
    }
}