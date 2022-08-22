using System.Collections.Concurrent;

namespace Chase.Nexus.Reference.Store
{
    public sealed class NexusMemoryStore
    {
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, NexusTypeStore>> records = new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, NexusTypeStore>>();

        public NexusMemoryStore() { }

        /// <summary>
        /// Fetches any matching targets from the persisted metrics
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityTypeId"></param>
        /// <param name="targets"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal List<T> GetMatches<T>(Guid entityTypeId, string target, Func<Guid, Guid, string, byte[], T> action)
        {
            List<T> res = new List<T>();
            ConcurrentDictionary<Guid, NexusTypeStore>? tmp;
            if (records.TryGetValue(entityTypeId, out tmp))
            {
                NexusTypeStore ts;
                NexusItem<byte[]>? rec;
                T item;
                foreach (KeyValuePair<Guid, NexusTypeStore> cur in tmp)
                {
                    ts = cur.Value;
                    if (ts.records.TryGetValue(target, out rec))
                    {
                        item = action(entityTypeId, cur.Key, target, rec.Data);
                        if (item != null)
                            res.Add(item);
                    }
                }
            }
            return res;
        }

        internal bool Replace(Guid entityTypeId, Guid communityId, Dictionary<string, byte[]> items)
        {
            if (items != null)
            {
                try
                {
                    ConcurrentDictionary<Guid, NexusTypeStore>? tmp;

                    if (!records.TryGetValue(entityTypeId, out tmp))
                    {
                        tmp = new ConcurrentDictionary<Guid, NexusTypeStore>();
                        records[entityTypeId] = tmp;
                    }

                    NexusTypeStore ts = new NexusTypeStore();
                    if (ts.Put(items))
                    {
                        tmp[communityId] = ts;
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        internal bool Put(Guid entityTypeId, Guid communityId, Dictionary<string, byte[]> items)
        {
            if (items != null)
            {
                try
                {
                    ConcurrentDictionary<Guid, NexusTypeStore>? tmp;

                    if (!records.TryGetValue(entityTypeId, out tmp))
                    {
                        tmp = new ConcurrentDictionary<Guid, NexusTypeStore>();
                        records[entityTypeId] = tmp;
                    }

                    NexusTypeStore? ts;

                    if (!tmp.TryGetValue(communityId, out ts))
                    {
                        ts = new NexusTypeStore();
                        tmp[communityId] = ts;
                    }

                    return ts.Put(items);
                }
                catch { }
            }
            return false;
        }

        internal void Remove(Guid entityTypeId, Guid communityId, IEnumerable<string> items)
        {
            if (items!=null)
            {
                ConcurrentDictionary<Guid, NexusTypeStore>? tmp;
                if (records.TryGetValue(entityTypeId, out tmp))
                {
                    NexusTypeStore? ts;
                    if (tmp.TryGetValue(communityId, out ts))
                    {
                        ts.RemoveIfExists(items);
                    }
                }
            }
        }

        internal bool Remove(Guid entityTypeId, Guid communityId)
        {
            ConcurrentDictionary<Guid, NexusTypeStore>? tmp;

            if (records.TryGetValue(entityTypeId, out tmp))
            {
                return tmp.TryRemove(communityId, out _);
            }

            return false;
        }

        internal Dictionary<string, byte[]>? GetMy(Guid entityTypeId, Guid communityId)
        {
            try
            {
                ConcurrentDictionary<Guid, NexusTypeStore>? tmp;

                if (records.TryGetValue(entityTypeId, out tmp))
                {
                    NexusTypeStore? ts;

                    if (tmp.TryGetValue(communityId, out ts))
                    {
                        return ts.Get();
                    }
                }
            }
            catch { }

            return null;
        }

        //TODO -- manage subscriptions so that a get only returns some channels
        internal IEnumerable<KeyValuePair<Guid, Dictionary<string, byte[]>>>? Get(Guid entityTypeId, Guid communityId)
        {
            try
            {
                ConcurrentDictionary<Guid, NexusTypeStore>? tmp;

                if (records.TryGetValue(entityTypeId, out tmp))
                {
                    List<KeyValuePair<Guid, Dictionary<string, byte[]>>> result = new List<KeyValuePair<Guid, Dictionary<string, byte[]>>>();
                    long total = 0;
                    foreach (KeyValuePair<Guid, NexusTypeStore> cur in tmp)
                    {
                        if (!communityId.Equals(cur.Key))
                        {
                            result.Add(new KeyValuePair<Guid, Dictionary<string, byte[]>>(cur.Key, cur.Value.Get()));
                            total += result[result.Count - 1].Value.Count;
                        }
                    }
                    Console.WriteLine("NexusMemoryStore.Get Got: " + result.Count + " communities " + total + " records");
                    return result;
                }
            }
            catch { }

            return null;
        }

        internal IEnumerable<Guid> GetEntityTypes(Guid communityId)
        {
            List<Guid> ids = new List<Guid>();
            foreach (Guid cur in records.Keys)
            {
                ids.Add(cur);
            }
            return ids;
        }

        private static readonly object syncRoot = new object();
        private static Task? t=null;
        private static readonly HashSet<NexusMemoryStore> scavengedInstances = new HashSet<NexusMemoryStore>();

        internal static void StartScavenge(NexusMemoryStore instance)
        {
            lock (syncRoot)
            {
                if (instance!=null)
                    scavengedInstances.Add(instance);

                if (t == null)
                {
                    t = new Task(ScavengeImpl, TaskCreationOptions.LongRunning);
                    t.Start();
                }
            }
        }

        //TODO -- load from config
        private static double ageOffSeconds = -2592000; //30 days in seconds

        private static void ScavengeImpl()
        {
            while (true)
            {
                Console.WriteLine("NexusMemoryStore: Scavenging");
                try
                {
                    foreach(NexusMemoryStore instance in scavengedInstances) //very small N nearly always <10, more like 2
                    {
                        foreach (ConcurrentDictionary<Guid, NexusTypeStore> cur in instance.records.Values) //small N, number of unique entityTypes order of 10 < N < 100
                        {
                            foreach (NexusTypeStore item in cur.Values) //moderate to large N depending on entityType and participants
                            {
                                item.Scavenge(ageOffSeconds);
                            }
                        }
                    }
                }
                catch { }

                Thread.Sleep(18000000); //5 hours in millis
            }
        }
    }
}
