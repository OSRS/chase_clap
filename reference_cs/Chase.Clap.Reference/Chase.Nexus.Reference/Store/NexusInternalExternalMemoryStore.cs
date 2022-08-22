namespace Chase.Nexus.Reference.Store
{
    public sealed class NexusInternalExternalMemoryStore
    {
        private readonly NexusMemoryStore internalStore = new NexusMemoryStore();
        private readonly NexusMemoryStore externalStore = new NexusMemoryStore();
        private readonly Guid localCommunityId;

        public NexusInternalExternalMemoryStore(Guid localCommunityId) { this.localCommunityId = localCommunityId; }

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
            HashSet<T> matches = new HashSet<T>();
            Append(matches, internalStore.GetMatches(entityTypeId, target, action));
            Append(matches, externalStore.GetMatches(entityTypeId, target, action));
            
            List<T> result = new List<T>();
            result.AddRange(matches);
            return result;
        }

        private void Append<T>(HashSet<T> matches, IEnumerable<T> result)
        {
            foreach(T item in result)
            {
                matches.Add(item);
            }
        }

        private void Append(Dictionary<Guid, Dictionary<string, byte[]>> matches, IEnumerable<KeyValuePair<Guid, Dictionary<string, byte[]>>>? result)
        {
            if (result != null)
            {
                foreach (KeyValuePair<Guid, Dictionary<string, byte[]>> item in result)
                {
                    if (matches.ContainsKey(item.Key))
                    {
                        Dictionary<string, byte[]> tmp = matches[item.Key];
                        if (tmp != null)
                        {
                            foreach (KeyValuePair<string, byte[]> pair in tmp)
                            {
                                tmp[pair.Key] = pair.Value;
                            }
                        }
                    }
                    else if (item.Value != null)
                        matches.Add(item.Key, item.Value);
                }
            }
        }

        internal bool Replace(Guid entityTypeId, Guid communityId, Dictionary<string, byte[]> items)
        {
            if (items != null)
            {
                if (communityId.Equals(localCommunityId))
                    internalStore.Replace(entityTypeId, communityId, items);
                else
                {
                    externalStore.Replace(entityTypeId, communityId, items);
                    internalStore.Remove(entityTypeId, communityId, items.Keys); //remove matching internal items
                }
            }
            return false;
        }

        internal bool Put(Guid entityTypeId, Guid communityId, Dictionary<string, byte[]> items)
        {
            if (items != null)
            {
                if (communityId.Equals(localCommunityId))
                    return internalStore.Put(entityTypeId, communityId, items);
                else
                {
                    bool res = externalStore.Put(entityTypeId, communityId, items);
                    internalStore.Remove(entityTypeId, communityId, items.Keys);
                    return res;
                }
            }
            return false;
        }

        internal bool Remove(Guid entityTypeId, Guid communityId)
        {
            return externalStore.Remove(entityTypeId, communityId);
        }

        internal Dictionary<string, byte[]>? GetMy(Guid entityTypeId, Guid communityId)
        {
            if (communityId.Equals(localCommunityId))
                return internalStore.GetMy(entityTypeId, communityId);
            return externalStore.GetMy(entityTypeId, communityId);
        }

        //TODO -- manage subscriptions so that a get only returns some channels
        internal IEnumerable<KeyValuePair<Guid, Dictionary<string, byte[]>>>? Get(Guid entityTypeId, Guid communityId)
        {
            Dictionary<Guid, Dictionary<string, byte[]>>? result = new Dictionary<Guid, Dictionary<string, byte[]>>();
            Append(result, internalStore.Get(entityTypeId, communityId));
            Append(result, externalStore.Get(entityTypeId, communityId));
            return result;
        }

        internal IEnumerable<Guid> GetEntityTypes(Guid communityId)
        {
            HashSet<Guid> ids = new HashSet<Guid>();
            Append(ids, internalStore.GetEntityTypes(communityId));
            Append(ids, externalStore.GetEntityTypes(communityId));
            return ids;
        }


        private static readonly object syncRoot = new object();
        public static void StartScavenge(NexusInternalExternalMemoryStore instance)
        {
            lock (syncRoot)
            {
                if (instance != null)
                {
                    NexusMemoryStore.StartScavenge(instance.internalStore);
                    NexusMemoryStore.StartScavenge(instance.externalStore);
                }
            }
        }
    }
}
