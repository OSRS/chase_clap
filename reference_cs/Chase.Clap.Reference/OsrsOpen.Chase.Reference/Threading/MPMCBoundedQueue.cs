using System.Collections.Concurrent;

namespace OsrsOpen.Chase.Reference.Threading
{
    public enum QueueFullBehavior
    {
        DropOnInsert,
        DequeueOnInsert,
        BlockInsert
    }

    public sealed class MPMCBoundedQueue<T>
    {
        private readonly ConcurrentQueue<T> inner = new ConcurrentQueue<T>();
        private readonly int maxItems;
        private readonly QueueFullBehavior behavior;
        private long items = 0;

        public long Count
        {
            get { return Volatile.Read(ref items); }
        }

        public MPMCBoundedQueue(int maxItems, QueueFullBehavior behavior)
        {
            if (maxItems > 0)
                this.maxItems = maxItems;
            else
                this.maxItems = 1024;
            this.behavior = behavior;
        }

        public MPMCBoundedQueue(int maxItems) : this(maxItems, QueueFullBehavior.BlockInsert)
        { }

        public MPMCBoundedQueue() : this(0, QueueFullBehavior.BlockInsert)
        { }

        private readonly object syncRoot = new object();
        private const int actionLim = int.MaxValue / 2;
        public bool Add(T item)
        {
            long count = Interlocked.Increment(ref items);
            if (count < maxItems)
                inner.Enqueue(item);
            else
            {
                if (behavior == QueueFullBehavior.BlockInsert)
                {
                    SpinWait waiter = new SpinWait();
                    while (maxItems <= count)
                    {
                        waiter.SpinOnce();
                        count = Volatile.Read(ref items);
                    }

                    inner.Enqueue(item);
                }
                else if (behavior == QueueFullBehavior.DropOnInsert)
                {
                    Interlocked.Decrement(ref items); //we do nothing, so remove our increment
                    return false;
                }
                else //dequeue on insert
                {
                    SpinWait waiter = new SpinWait();
                    while (!TryRemove(out _))
                    {
                        waiter.SpinOnce();
                    }
                    inner.Enqueue(item);
                }
            }

            return true;
        }

        internal void AddOverflow(T item)
        {
            Interlocked.Increment(ref items);
            inner.Enqueue(item);
        }

        public bool TryRemove(out T? item)
        {
            if (inner.TryDequeue(out item))
            {
                Interlocked.Decrement(ref items);
                return true;
            }
            return false;
        }

        public T? Remove()
        {
            T? res;

            if (!TryRemove(out res))
            {
                SpinWait waiter = new SpinWait();
                waiter.SpinOnce();
                while (!TryRemove(out res))
                {
                    waiter.SpinOnce();
                }
            }

            return res;
        }
    }
}
