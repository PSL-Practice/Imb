using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Data.RecordStreamImpl;
using Imb.Utils;

namespace Imb.DropHandling.DataHandlers
{
    public class FileDropDataHandler : IDataHandler
    {
        public bool TryGetData(IDataObject dataObject, out DropArgs output)
        {
            var data = dataObject.GetData("FileDrop") as string[];
            if (data != null)
            {
                output = new DropArgs();
                output.FileList = data.ToList();
                return true;
            }

            output = null;
            return false;
        }
    }

    public class URLDropDataHandler : IDataHandler
    {
        public bool TryGetData(IDataObject dataObject, out DropArgs output)
        {
            var data = dataObject.GetData("Text")as string;
            if (data != null)
            {
                using (var client = new WebClient())
                {
                    var bytes = client.DownloadData(data);
                    output = new DropArgs()
                    {
                        Data = bytes,
                        OriginalPath = data
                    };
                    return true;
                }

            }
            output = null;
            return false;
        }
    }
}
