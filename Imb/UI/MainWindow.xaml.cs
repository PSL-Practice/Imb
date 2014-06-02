using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Imb.ErrorHandling;
using Imb.LibrarySelection;
using Imb.Utils;
using Imb.ViewModels;

namespace Imb.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; set; }
        private readonly App _app;
        private readonly WindowPositionSettings _windowPositionSettings;
        private MainVm _vm;

        public MainWindow()
        {
            InitializeComponent();
            _app = (App)Application.Current;

            var fileValidator = new FileValidator();
            var errorHandler = new ErrorHandlerView();
            var librarySelector = new LibrarySelector(fileValidator, errorHandler, Dispatcher, _app.EventAggregator);
            _vm = new MainVm(new LibraryLocationDialog(), librarySelector, errorHandler, fileValidator, _app.AppSettings, _app.EventAggregator);
            DataContext = _vm;
            Instance = this;


            var positionData = _app.AppSettings.WindowPositions == null
                                   ? null
                                   : _app.AppSettings.WindowPositions
                                         .FirstOrDefault(w => w.WindowId == "MainWindow");
            _windowPositionSettings = new WindowPositionSettings(positionData, 0.7);
            _windowPositionSettings.AttachWindow(this);
            if (positionData == null)
            {
                _windowPositionSettings.PositionData.WindowId = "MainWindow";
                if (_app.AppSettings.WindowPositions == null)
                    _app.AppSettings.WindowPositions = new List<WindowPositionData>();
                _app.AppSettings.WindowPositions.Add(_windowPositionSettings.PositionData);
            }

            _app.MainWindow = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm.AppClosing();
        }

        public void Activate(IList<string> args)
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Activate();
        }

    }
}
