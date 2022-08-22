using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsrsOpen.Chase.Reference.Threading
{
    public abstract class WorkItemBase : ICancellableWorkItem
    {
        public TaskRunState State
        {
            get { return WorkResult.State; }
        }

        public Exception Exception
        {
            get { return WorkResult.Exception; }
        }

        protected internal abstract WorkResult WorkResult { get; }

        public void Cancel()
        {
            TaskRunState state = WorkResult.State;
            if (state == TaskRunState.Running || state == TaskRunState.NotStarted)
                this.WorkResult.State = TaskRunState.Cancelled;
        }

        public abstract void Run();

        internal WorkItemBase() { }
    }

    public abstract class WorkItem : WorkItemBase
    {
        protected internal override WorkResult WorkResult
        {
            get;
        } = new WorkResult();
    }

    public abstract class WorkItemBase<T> : WorkItemBase, ICancellableWorkItem<T>
    {
        private readonly WorkResult<T> result = new WorkResult<T>();

        protected void SetResult(T value)
        {
            result.Result = value;
        }

        protected internal override WorkResult WorkResult
        {
            get { return result; }
        }

        public T Result
        { get { return result.Result; } }
    }
}
