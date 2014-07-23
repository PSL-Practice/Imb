using System.IO;
using Gat.Controls;
using Imb.UI;

namespace Imb.ViewModels
{
    public class LibraryLocationDialog : ILibraryLocationDialog
    {
        public string GetNewLibraryLocation(string libraryPath)
        {
            // Initializing Open Dialog
            var openDialog = new OpenDialogView();
            var vm = (OpenDialogViewModel)openDialog.DataContext;
            vm.IsDirectoryChooser = true;
            vm.SelectedFilePath = libraryPath;

            // Setting window properties
            vm.Owner = MainWindow.Instance;
            vm.StartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

            // Show locationDialog and take result into account
            bool? result = vm.Show();
            if (result == true)
            {
                // Get selected file path
                if (vm.SelectedFolder != null)
                    return Path.Combine(vm.SelectedFolder.Path, vm.SelectedFilePath ?? string.Empty);
                return vm.SelectedFilePath;
            }
            return null;
        }

        public string GetExistingLibraryLocation(string libraryPath)
        {
            // Initializing Open Dialog
            var openDialog = new OpenDialogView();
            var vm = (OpenDialogViewModel)openDialog.DataContext;
            vm.IsDirectoryChooser = true;
            vm.SelectedFilePath = libraryPath;

            // Setting window properties
            vm.Owner = MainWindow.Instance;
            vm.StartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

            // Show locationDialog and take result into account
            bool? result = vm.Show();
            if (result == true)
            {
                // Get selected file path
                if (vm.SelectedFolder != null)
                    return Path.Combine(vm.SelectedFolder.Path, vm.SelectedFilePath ?? string.Empty);
                return vm.SelectedFilePath;
            }
            return null;
        }

        public string GetFileLocation()
        {
            // Initializing Open Dialog
            var openDialog = new OpenDialogView();
            var vm = (OpenDialogViewModel)openDialog.DataContext;

            // Setting window properties
            vm.Owner = MainWindow.Instance;
            vm.StartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

            // Show locationDialog and take result into account
            bool? result = vm.Show();
            if (result == true)
            {
                // Get selected file path
                //var path = Path.Combine(vm.SelectedFolder);
                if (vm.SelectedFolder != null)
                    return Path.Combine(vm.SelectedFolder.Path, vm.SelectedFilePath ?? string.Empty);
                return vm.SelectedFilePath;
            }
            return null;
        }
    }
}