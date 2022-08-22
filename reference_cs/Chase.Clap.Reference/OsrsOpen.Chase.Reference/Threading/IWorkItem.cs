namespace OsrsOpen.Chase.Reference.Threading
{
    public enum TaskRunState
    {
        Unknown = 0,
        NotStarted = 1,
        Running = 2,
        Complete = 3,
        Cancelled = 4,
        Failed = 5
    }

    public interface ICancellableWorkItem<T> : IWorkItem<T>, ICancellableWorkItem
    { }

    public interface ICancellableWorkItem : IWorkItem
    {
        void Cancel();
    }

    public interface IWorkItem
    {
        TaskRunState State
        {
            get;
        }

        void Run();
    }

    public interface IWorkItem<T> : IWorkItem
    {
        T Result { get; }
    }
}
