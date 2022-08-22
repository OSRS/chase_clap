using OsrsOpen.Chase.Reference;
using OsrsOpen.Chase.Reference.Processing;
using OsrsOpen.Chase.Reference.Threading;

namespace Chase.Clap.Reference.Processing
{
    internal sealed class ClapProcessorHost : IPuttable<ProcessingEntityInstant>
    {
        private readonly int maxItemsPerRun;
        private readonly ClapFlowProcessorBase inner;
        private readonly MPMCBoundedQueue<ProcessingEntityInstant> items;

        public RunState State
        {
            get { return inner.State; }
        }

        public void Put(ProcessingEntityInstant item)
        {
            items.Add(item); //note this will block if full
            //if the number of threads in the pool used is less than the number of processor hosts,
            //and throughput is low - all threads could end up blocked

            //we currently only partially mitigate this at the leftmost edge by blocking early if we push too much data in
            //this still assumes a relatively high thread count relative to the number of processors
        }

        public ClapProcessorHost(int maxItemsPerRun, ClapFlowProcessorBase inner, int maxQueueSize)
        {
            this.maxItemsPerRun = maxItemsPerRun;
            this.inner = inner;
            this.items = new MPMCBoundedQueue<ProcessingEntityInstant>(maxQueueSize);
        }

        public bool JobWork()
        {
            if (State == RunState.Running)
            {
                int exCt = 0;
                ProcessingEntityInstant? item;

                try
                {
                    while (items.TryRemove(out item))
                    {
                        if (item != null)
                        {
                            exCt++;
                            try
                            {
                                inner.Put(item);
                            }
                            catch { }

                            if (exCt > maxItemsPerRun)
                                return State == RunState.Running;
                        }
                    }
                }
                catch { }

                return State == RunState.Running;
            }
            return false;
        }
    }
}
