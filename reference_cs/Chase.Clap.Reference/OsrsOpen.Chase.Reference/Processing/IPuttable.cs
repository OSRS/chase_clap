namespace OsrsOpen.Chase.Reference.Processing
{
    public interface IPuttable<in T> : IStateful
    {
        void Put(T item);
    }
}
