namespace Imb.Data
{
    internal interface ILibrary
    {
        ITagCache TagCache { get; }
        IPathsCache PathsCache { get; }
        IBinariesCache BinariessCache { get; }
    }
}