namespace OsrsOpen.Chase.Reference.Threading
{
    public static class WorkPoolUtils
    {
        public static ICancellableWorkItem? QueueTask(System.Action item)
        {
            if (item != null)
                return WorkPool.Default.QueueTask(item);
            return null;
        }

        public static ICancellableWorkItem<T>? QueueTask<T>(System.Func<T> item)
        {
            if (item != null)
                return WorkPool.Default.QueueTask(item);
            return null;
        }

        public static ICancellableWorkItem? QueueEventLoopTask(System.Func<bool> item)
        {
            if (item != null)
                return WorkPool.Default.QueueEventLoopTask(item);
            return null;
        }

        public static ICancellableWorkItem? QueueTask(this WorkPool pool, System.Action item)
        {
            if (pool != null && item != null)
            {
                WorkActionWrapper tmp = new WorkActionWrapper(item);
                pool.QueueWork(tmp, 1);
                return tmp;
            }
            return null;
        }

        public static ICancellableWorkItem<T>? QueueTask<T>(this WorkPool pool, System.Func<T> item)
        {
            if (pool != null && item != null)
            {
                WorkFuncWrapper<T> tmp = new WorkFuncWrapper<T>(item);
                pool.QueueWork(tmp, 1);
                return tmp;
            }
            return null;
        }

        public static ICancellableWorkItem? QueueEventLoopTask(this WorkPool pool, System.Func<bool> item)
        {
            if (pool != null && item != null)
            {
                EventLoopWorkItem tmp = new EventLoopWorkItem(item);
                pool.QueueWork(tmp, 1);
                return tmp;
            }
            return null;
        }
    }

    internal sealed class WorkFuncWrapper<T> : WorkItemBase<T>
    {
        private readonly Func<T> work;

        internal WorkFuncWrapper(Func<T> work)
        {
            this.work = work;
        }

        public override void Run()
        {
            try
            {
                this.WorkResult.State = TaskRunState.Running;
                T res = work();
                this.SetResult(res);
                this.WorkResult.State = TaskRunState.Complete;
            }
            catch (Exception e)
            {
                this.WorkResult.Exception = e;
                this.WorkResult.State = TaskRunState.Failed;
            }
        }
    }

    internal sealed class WorkActionWrapper : WorkItem
    {
        private readonly Action work;

        internal WorkActionWrapper(Action work)
        {
            this.work = work;
        }

        public override void Run()
        {
            try
            {
                this.WorkResult.State = TaskRunState.Running;
                work();
                this.WorkResult.State = TaskRunState.Complete;
            }
            catch (Exception e)
            {
                this.WorkResult.Exception = e;
                this.WorkResult.State = TaskRunState.Failed;
            }
        }
    }
}
