using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Imb.Utils
{
    public class FileValidator : IFileValidator
    {
        public bool Validate(string path)
        {
            return Validate(File.ReadAllBytes(path));
        }

        public bool Validate(byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    var bi = new BitmapImage();

                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public interface IFileValidator
    {
        bool Validate(string path);
        bool Validate(byte[] data);
    }
}
