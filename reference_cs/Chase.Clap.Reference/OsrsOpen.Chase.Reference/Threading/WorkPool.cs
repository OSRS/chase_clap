namespace OsrsOpen.Chase.Reference.Threading
{
    public sealed class WorkPool : IDisposable
    {
        private readonly ushort numWorkers;
        private int workersCount = 0;

        private bool running = true;

        private int queueLimit;
        public int QueueLimit
        {
            get { return queueLimit; }
        }

        private readonly MPMCBoundedQueue<WorkItemBase> tasks;
        public long Count
        {
            get { return tasks.Count; }
        }

        private readonly SpinWait queueWaiter = new SpinWait();

        public bool QueueWork(WorkItemBase item)
        {
            if (item != null && queueLimit > 0 && Volatile.Read(ref this.running))
                return tasks.Add(item);
            return false;
        }

        public bool QueueWork(WorkItemBase item, int maxTries)
        {
            if (item != null && queueLimit > 0 && Volatile.Read(ref this.running))
            {
                if (maxTries < 1)
                    maxTries = int.MaxValue;
                int curTry = 0;

                while (curTry < maxTries)
                {
                    if (tasks.Add(item))
                        return true;

                    queueWaiter.SpinOnce();
                    curTry++;
                }
            }
            return false;
        }

        private void Start()
        {
            Thread th;
            for (int i = 0; i < numWorkers; i++)
            {
                th = new Thread(Run);
                th.IsBackground = true;
                Interlocked.Increment(ref workersCount);
                th.Start();
            }
        }

        private void Run()
        {
            SpinWait taskWaiter = new SpinWait(); //note this is now thread-specific so each one waits independently
            while (Volatile.Read(ref this.running))
            {
                WorkItemBase? item = null;
                try
                {
                    if (tasks.TryRemove(out item))
                    {
                        if (item.State == TaskRunState.NotStarted || item.State == TaskRunState.Running)
                            item.Run();

                        if (item.State == TaskRunState.Running) //returned with more work to do, such as a FSM - so we re-enqueue
                            tasks.AddOverflow(item); //avoids the too-full block
                    }
                    else
                        taskWaiter.SpinOnce();
                }
                catch (Exception e)
                {
                    if (item != null)
                    {
                        item.WorkResult.Exception = e;
                        item.WorkResult.State = TaskRunState.Failed;
                    }
                }

                if (!Thread.CurrentThread.IsBackground) //ensure the task didn't make it a foreground thread
                    Thread.CurrentThread.IsBackground = true;
            }
            Interlocked.Decrement(ref workersCount);
            //fall through here allows thread to die-off
        }

        public void Stop(int drainWaitTimeMillis)
        {
            StopImpl(true, drainWaitTimeMillis);
        }

        public void Stop(bool drain)
        {
            StopImpl(drain, 300000); //5 minutes
        }

        private readonly object syncRoot = new object();

        private void StopImpl(bool drain, int waitTimeMillis)
        {
            lock (syncRoot)
            {
                if (Volatile.Read(ref this.running))
                {
                    if (drain)
                    {
                        if (waitTimeMillis <= 0)
                            waitTimeMillis = 0;

                        DateTime st = DateTime.UtcNow;
                        //NOTE: if there are re-queuing jobs, this may never stop
                        while (this.Count > 0)
                        {
                            queueWaiter.SpinOnce();
                            if (DateTime.UtcNow.Subtract(st).TotalMilliseconds > waitTimeMillis)
                                break;
                        }
                    }

                    Volatile.Write(ref this.running, false);

                    while (workersCount > 0)
                        queueWaiter.SpinOnce(); //wait for all threads to be idled and die-off
                }
            }
        }

        public static WorkPool Create(ushort numWorkers, int queueLimit)
        {
            if (queueLimit <= 0 || queueLimit > int.MaxValue)
                queueLimit = int.MaxValue;

            if (numWorkers == 0)
                return new WorkPool((ushort)Environment.ProcessorCount, queueLimit);

            return new WorkPool(numWorkers, queueLimit);
        }

        public void Dispose()
        {
            try
            {
                StopImpl(false, 0);
            }
            catch { }
        }

        private WorkPool(ushort numWorkers, int queueLimit)
        {
            this.numWorkers = numWorkers;
            this.queueLimit = queueLimit;
            this.tasks = new MPMCBoundedQueue<WorkItemBase>(queueLimit, QueueFullBehavior.DropOnInsert);

            Start();
        }

        private static WorkPool? instance = null;
        public static WorkPool Default
        {
            get
            {
                if (instance == null)
                    instance = new WorkPool((ushort)Environment.ProcessorCount, ushort.MaxValue);

                return instance;
            }
        }
    }
}
