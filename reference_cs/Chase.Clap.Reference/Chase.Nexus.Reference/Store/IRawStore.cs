namespace Chase.Nexus.Reference.Store
{
    public interface IRawStore
    {
        bool Put(Guid entityTypeId, Guid communityId, Dictionary<string, byte[]> items);

        bool Replace(Guid entityTypeId, Guid communityId, Dictionary<string, byte[]> items);

        bool Remove(Guid entityTypeId, Guid communityId);

        Dictionary<string, byte[]> GetMy(Guid entityTypeId, Guid communityId);

        IEnumerable<KeyValuePair<Guid, Dictionary<string, byte[]>>> Get(Guid entityTypeId, Guid communityId);

        IEnumerable<Guid> GetEntityTypes(Guid communityId);
    }

    public interface ISimulationRawStore
    {
        bool Put(Guid simulationId, Guid entityTypeId, Guid communityId, Dictionary<string, byte[]> items);

        bool Replace(Guid simulationId, Guid entityTypeId, Guid communityId, Dictionary<string, byte[]> items);

        bool Remove(Guid simulationId, Guid entityTypeId, Guid communityId);

        Dictionary<string, byte[]> GetMy(Guid simulationId, Guid entityTypeId, Guid communityId);

        IEnumerable<KeyValuePair<Guid, Dictionary<string, byte[]>>> Get(Guid simulationId, Guid entityTypeId, Guid communityId);

        IEnumerable<Guid> GetEntityTypes(Guid simulationId, Guid communityId);
    }
}
