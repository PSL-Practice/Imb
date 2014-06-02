using System.Collections.Generic;
using System.Linq;
using Imb.Data;

namespace TestImb.Mocks
{
    public class MockTagsCache : ITagCache
    {
        public Dictionary<string, int> Tags = new Dictionary<string, int>();
        public int Next = 1;

        public int AddOrGet(string tag)
        {
            int id;
            if (Tags.TryGetValue(tag, out id))
                return id;

            Tags[tag] = Next;
            return Next++;
        }

        public string Get(int id)
        {
            return Tags.FirstOrDefault(t => t.Value == id).Key ?? "??";
        }
    }
}