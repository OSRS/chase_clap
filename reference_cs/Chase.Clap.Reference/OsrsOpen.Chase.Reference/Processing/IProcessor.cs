namespace OsrsOpen.Chase.Reference.Processing
{
    public interface IStateful
    {
        RunState State { get; }
    }

    public interface IStoppable : IStateful
    {
        void Stop();
    }

    public interface IStartable : IStateful
    {
        void Start();
    }

    public interface IProcessor : IStartable, IStoppable
    {
    }
}
