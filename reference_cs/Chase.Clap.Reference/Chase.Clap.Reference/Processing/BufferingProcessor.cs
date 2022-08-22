using OsrsOpen.Chase.Reference;
using OsrsOpen.Chase.Reference.Processing;

namespace Chase.Clap.Reference.Processing
{
    public sealed class BufferingProcessor : ClapFlowProcessorBase
    {
        private readonly OrderedLinkedList<ProcessingEntityInstant> items = new OrderedLinkedList<ProcessingEntityInstant>(new PEComparer());
        private readonly int bufferSize;

        public BufferingProcessor(int bufferSize)
        {
            if (bufferSize > 0)
                this.bufferSize = bufferSize;
            else
                this.bufferSize = ushort.MaxValue;
        }

        public override ProcessingEntityInstant Handle(ProcessingEntityInstant item)
        {
            throw new NotImplementedException();
        }

        public override void Put(ProcessingEntityInstant item)
        {
            if (item != null)
            {
                while (items.Count >= this.bufferSize)
                {
                    ProcessingEntityInstant? it = this.items.Remove();
                    if (it != null)
                        this.Forward(it);
                    else
                        this.Flow?.ExitRecord();
                }
                this.items.Add(item);
            }
        }

        public override void Start()
        {
            this.State = RunState.Running;
        }

        public override void Stop()
        {
            while (this.items.Count > 0)
            {
                ProcessingEntityInstant? it = this.items.Remove();
                if (it != null)
                    this.Forward(it);
                else
                    this.Flow?.ExitRecord();
            }

            this.State = RunState.Stopped;
        }
    }
}
