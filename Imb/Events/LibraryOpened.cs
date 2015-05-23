namespace Imb.Events
{
    public sealed class LibraryOpened
    {
        public string Name { get; set; }

        public LibraryOpened(string name)
        {
            Name = name;
        }
    }
}