using System.Windows;
using Imb.EventAggregation;
using Imb.Utils;
using Utils;

namespace Imb
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly UserSettings _userSettings;

        public App()
        {
            _userSettings = new UserSettings();

            ProcessSettings();

            DropHandler.RegisterDataHandlers();
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            _userSettings.Save();
        }

        private void ProcessSettings()
        {
            EventAggregator = new EventAggregator();
            AppSettings = new ImbSettings();
            _userSettings.AttachConfigObject(AppSettings);
            _userSettings.Load();
        }

        public ImbSettings AppSettings { get; set; }
        public IEventAggregator EventAggregator { get; private set; }
    }
}
