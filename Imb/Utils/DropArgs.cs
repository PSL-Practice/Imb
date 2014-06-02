using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace Imb.Utils
{
    public class DropArgs
    {
        public static implicit operator DropArgs(DragEventArgs args)
        {
            foreach (var format in args.Data.GetFormats())
            {
                Debug.Print(format);
            }

            foreach (var dataHandler in DropHandler.DataHandlers)
            {
                DropArgs output;
                if (dataHandler.TryGetData(args.Data, out output))
                    return output;
            }

            return new DropArgs()
            {
                Error = "The data is not available in a format that can be imported."
            };
        }

        public string Error { get; set; }

        public string Url { get; set; }

        public List<string> FileList { get; set; }
        public byte[] Data { get; set; }
        public string OriginalPath { get; set; }
    }

    public interface IDropData
    {
        T GetData<T>(string format);
    }
}