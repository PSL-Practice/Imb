using System;
using System.Collections.Generic;
using System.Linq;

namespace TestImb.Mocks
{
    internal static class TestIds
    {
        public static Dictionary<Guid, string> Ids;
 
        public static string Id(Guid id)
        {
            string name;
            if (Ids.TryGetValue(id, out name))
                return name;

            return "missing";
        }

        static TestIds()
        {
            Ids = Enumerable.Range(0, 10)
                .Select(i => new {Id = Guid.NewGuid(), Name = string.Format("Id{0}", i)})
                .ToDictionary(d => d.Id, d => d.Name);

            Ids[Guid.Empty] = "Empty";
        }

        public static Guid GetId(int i)
        {
            return Ids.Keys.ToList()[i];
        }
    }
}