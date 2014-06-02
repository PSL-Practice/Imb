using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Data.Utilities;
using Imb.Annotations;
using Imb.Caching;
using Imb.Data.View;
using IClaim = Imb.Caching.IClaim;

namespace Imb.ViewModels
{
    public class DisplayVm : IDisplay, INotifyPropertyChanged
    {
        private readonly LoadedBinaryCache _binariesCache;
        private IClaim _currentBinary;
        private ImageSource _image;

        public DisplayVm(LoadedBinaryCache binariesCache)
        {
            _binariesCache = binariesCache;
        }

        public ImageSource Image
        {
            get { return _image; }
            set
            {
                if (Equals(value, _image)) return;
                _image = value;
                OnPropertyChanged();
            }
        }

        public void Show(LibraryViewNode node)
        {
            try
            {
                if (node is LibraryFileNode)
                {
                    using (var guard = new MultiGuard(1))
                    {
                        IClaim newBinary;
                        guard.Accept(newBinary = _binariesCache.GetBinary(node.Id));
                        if (newBinary != null)
                        {
                            ChangeClaim(newBinary);
                            guard.Commit();

                            LoadImage(newBinary.Binary);
                        }
                    }
                }
                else
                {
                    Image = null;
                }
            }
// ReSharper disable once EmptyGeneralCatchClause
            catch
            {

            }
        }

        private void LoadImage(byte[] binary)
        {
            using (var ms = new MemoryStream(binary))
            {
                var bi = new BitmapImage();

                try
                {
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = ms;
                    bi.EndInit();
                
                    bi.Freeze();
                
                    Image = bi;
                }
                catch
                {
                    Image = null;
                }
            }

        }

        private void ChangeClaim(IClaim newBinary)
        {
            if (_currentBinary != null)
            {
                _currentBinary.Dispose();
                _currentBinary = newBinary;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface IDisplay
    {
        void Show(LibraryViewNode id);
    }
}
