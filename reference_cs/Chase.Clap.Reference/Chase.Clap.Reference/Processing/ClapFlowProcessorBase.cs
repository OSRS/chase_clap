using Chase.Clap.Reference.Flow;
using OsrsOpen.Chase.Reference;
using OsrsOpen.Chase.Reference.Processing;

namespace Chase.Clap.Reference.Processing
{
    public abstract class ClapFlowProcessorBase : IDataProcessor<ProcessingEntityInstant>
    {
        public RunState State
        {
            get;
            protected set;
        }

        private ClapFlowProcessorBase? next;
        internal ClapFlowProcessorBase? Next
        {
            get { return next; }
            set { if (value != null) next = value; }
        }

        internal ClapFlow? Flow
        {
            get;
            set;
        }

        protected void Forward(ProcessingEntityInstant item)
        {
            if (next != null)
                next.Put(item);
        }

        public abstract ProcessingEntityInstant Handle(ProcessingEntityInstant item);

        public abstract void Put(ProcessingEntityInstant item);

        public abstract void Start();

        public abstract void Stop();
    }

    public abstract class ClapSimpleFlowProcessorBase : ClapFlowProcessorBase
    {
        public sealed override void Put(ProcessingEntityInstant item)
        {
            if (item!=null)
            {
                ProcessingEntityInstant res = Handle(item);
                if (res != null)
                    Forward(res);
                else
                    Flow?.ExitRecord();
            }
        }
    }

    public abstract class ClapTypeFilteredFlowProcessorBase : ClapFlowProcessorBase
    {
        protected abstract bool AcceptType(Guid etid);

        public sealed override void Put(ProcessingEntityInstant item)
        {
            if (item != null)
            {
                if (AcceptType(item.EntityTypeId))
                {
                    ProcessingEntityInstant res = Handle(item);
                    if (res != null)
                        Forward(res);
                    else
                        Flow?.ExitRecord();
                }
                else
                    Forward(item);
            }
        }
    }

    internal sealed class ClapFlowTerminator : ClapFlowProcessorBase
    {
        public override ProcessingEntityInstant Handle(ProcessingEntityInstant item)
        {
            throw new NotImplementedException();
        }

        public override void Put(ProcessingEntityInstant item)
        {
            Flow?.ExitRecord(); //that's all we do
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
