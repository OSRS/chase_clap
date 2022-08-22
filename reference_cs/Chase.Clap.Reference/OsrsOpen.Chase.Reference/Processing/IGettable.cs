namespace OsrsOpen.Chase.Reference.Processing
{
    public interface IGettable<out T> : IStateful
    {
        T Get();
    }
}
