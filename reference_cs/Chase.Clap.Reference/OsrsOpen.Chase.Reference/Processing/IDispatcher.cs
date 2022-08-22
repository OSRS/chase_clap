namespace OsrsOpen.Chase.Reference.Processing
{
    public interface IDispatcher<T> : IStateful
    {
        void Add(IPuttable<T> forward);
    }
}
