namespace OsrsOpen.Chase.Reference.Processing
{
    public interface IForwardableProcessor<F, T> : IDataProcessor<F>, IDispatchingProcessor<T>
    {
    }
}
