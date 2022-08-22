namespace OsrsOpen.Chase.Reference.Threading
{
    public class WorkResult
    {
        public TaskRunState State
        {
            get;
            set;
        }

        public Exception? Exception
        {
            get;
            set;
        }

        internal WorkResult()
        { this.State = TaskRunState.NotStarted; }
    }

    public sealed class WorkResult<T> : WorkResult
    {
        public T? Result
        {
            get;
            set;
        }

        internal WorkResult()
        { }
    }
}
