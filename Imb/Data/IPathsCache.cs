using System.Collections.Generic;

namespace Imb.Data
{
    public interface IPathsCache
    {
        int AddOrGet(IEnumerable<int> tags);
        int AddOrGet(IEnumerable<string> tags);
        int[] Get(int id);
    }
}