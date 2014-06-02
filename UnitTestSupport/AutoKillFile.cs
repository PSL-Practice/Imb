using System;
using System.IO;

namespace UnitTestSupport
{
	public class AutoKillFile : IDisposable
	{
		private readonly string _path;
		private bool _created;
		public AutoKillFile(string path, bool deleteIfPresent = false)
		{
			_path = path;
			if (File.Exists(path))
			{
				if (deleteIfPresent)
					File.Delete(path);

				_created = false;
			}
			else
				_created = true;
		}

		public string Path { get { return _path; } }


		public void Dispose()
		{
			if (_created && File.Exists(_path))
			{
				var startTime = DateTime.Now;
				do
				{
					try
					{
						File.Delete(_path);
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
