namespace OsrsOpen.Chase.Reference.Threading
{
    public class EventLoopWorkItem : WorkItem
    {
        private readonly Func<bool> work;

        public override void Run()
        {
            try
            {
                if (this.WorkResult.State == TaskRunState.NotStarted)
                    this.WorkResult.State = TaskRunState.Running;

                //note if the state is set to cancel we'll stop
                if (this.WorkResult.State == TaskRunState.Running && work())
                    return; //the pool will see we are still "running" and automatically requeue us

                if (this.WorkResult.State == TaskRunState.Running)
                    this.WorkResult.State = TaskRunState.Complete;
            }
            catch (Exception e)
            {
                this.WorkResult.Exception = e;
                this.WorkResult.State = TaskRunState.Failed;
            }
        }

        public EventLoopWorkItem(Func<bool> work)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));
            this.work = work;
        }
    }
}
