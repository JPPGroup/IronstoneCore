using System.Collections.Generic;
using System.Linq;

namespace Jpp.Ironstone.Core
{
    public class Consensus
    {
        Dictionary<string, int> _entries;

        public Consensus()
        {
            _entries = new Dictionary<string, int>();
        }

        public void Add(string s)
        {
            if (_entries.ContainsKey(s))
            {
                _entries[s] = _entries[s]++;
            }
            else
            {
                _entries.Add(s, 1);
            }
        }

        public string GetConsensus()
        {
            var order = _entries.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value).First();
            return order.Key;            
        }
    }
}
