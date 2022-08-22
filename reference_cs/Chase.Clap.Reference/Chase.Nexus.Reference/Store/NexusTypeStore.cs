using System.Collections.Concurrent;

namespace Chase.Nexus.Reference.Store
{
    public sealed class NexusTypeStore
    {
        internal readonly ConcurrentDictionary<string, NexusItem<byte[]>> records = new ConcurrentDictionary<string, NexusItem<byte[]>>();

        internal void RemoveIfExists(string key)
        {
            records.TryRemove(key, out _);
        }

        internal void RemoveIfExists(IEnumerable<string> items)
        {
            try
            {
                foreach (string cur in items)
                {
                    RemoveIfExists(cur);
                }
            }
            catch { } //if items is null
        }

        internal void RemoveIfExists(Dictionary<string, byte[]> items)
        {
            RemoveIfExists(items.Keys);
        }

        internal bool Put(Dictionary<string, byte[]> items)
        {
            try
            {
                foreach (KeyValuePair<string, byte[]> cur in items)
                {
                    Put(cur.Key, cur.Value);
                }
                return true;
            }
            catch { }
            return false;
        }

        internal bool Put(string key, byte[] item)
        {
            try
            {
                records[key] = new NexusItem<byte[]>(item);
                return true;
            }
            catch { }
            return false;
        }

        internal Dictionary<string, byte[]> Get()
        {
            Dictionary<string, byte[]> tmp = new Dictionary<string, byte[]>();
            foreach (KeyValuePair<string, NexusItem<byte[]>> value in records)
            {
                tmp[value.Key] = value.Value.Data;
            }
            return tmp;
        }

        internal void Scavenge(double ageOffSeconds)
        {
            List<string> removes = new List<string>(251);

            DateTime thresh = DateTime.UtcNow.AddSeconds(ageOffSeconds);
            foreach (KeyValuePair<string, NexusItem<byte[]>> cur in records)
            {
                if (cur.Value.ReceiptTime < thresh)
                    removes.Add(cur.Key);
            }

            if (removes.Count > 0)
            {
                NexusItem<byte[]>? tmp;
                foreach (string cur in removes)
                {
                    if (records.TryRemove(cur, out tmp))
                    {
                        if (tmp.ReceiptTime > thresh)
                            records[cur] = tmp; //allow for adding back if updated in interim
                    }
                }
            }
        }
    }
}
