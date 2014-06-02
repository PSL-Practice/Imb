using System;
using System.IO;

namespace UnitTestSupport
{
    public class AutoKillFolder : IDisposable
    {
        private readonly string _path;
        private readonly bool _created;
        public AutoKillFolder(string path, bool deleteIfPresent = false)
        {
            _path = path;
            if (Directory.Exists(path))
            {
                if (deleteIfPresent)
                {
                    Console.WriteLine("Delete: {0}", System.IO.Path.GetFullPath(path));
                    Directory.Delete(path, true);
                }

                _created = false;
            }
            else
                _created = true;
        }

        public string Path { get { return _path; } }


        public void Dispose()
        {
            if (_created && Directory.Exists(_path))
            {
                var startTime = DateTime.Now;
                do
                {
                    try
                    {
                        Directory.Delete(_path, true);
                        break;
                    }
                    catch (IOException)
                    {
                    }
                } while ((DateTime.Now - startTime).TotalMilliseconds < 500);
            }
            GC.SuppressFinalize(this);
        }
    }
}