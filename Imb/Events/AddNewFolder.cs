using System;

namespace Imb.Events
{
    public sealed class AddNewFolder
    {
        public string[] Path { get; private set; }

        public AddNewFolder(string[] path)
        {
            Path = path ?? new string[] {};
        }
    }
}