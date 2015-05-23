using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;
using Imb.UI;

namespace Imb.ViewModels
{
    public class LibraryLocationDialog : ILibraryLocationDialog
    {
        public string GetNewLibraryLocation(string libraryPath, Window window = null)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder to contain the new library.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

            dialog.ShowNewFolderButton = true;
            dialog.SelectedPath = libraryPath;

            if ((dialog.ShowDialog(window) ?? false) && dialog.SelectedPath != null)
                    return dialog.SelectedPath;
            
            return null;
        }

        public string GetExistingLibraryLocation(string libraryPath, Window window = null)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select the folder containing the library.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = libraryPath;

            if ((dialog.ShowDialog(window) ?? false) && dialog.SelectedPath != null)
                return dialog.SelectedPath;

            return null;
        }

        public string GetFileLocation(Window window = null)
        {
            var dialog = new VistaOpenFileDialog();
            dialog.CheckFileExists = true;
            
            if ((dialog.ShowDialog(window) ?? false) && dialog.FileName != null)
                return dialog.FileName;

            return null;
        }
    }
}