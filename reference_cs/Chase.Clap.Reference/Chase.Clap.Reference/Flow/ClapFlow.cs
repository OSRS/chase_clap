using Chase.Clap.Reference.Processing;
using OsrsOpen.Chase.Reference;
using OsrsOpen.Chase.Reference.Processing;

namespace Chase.Clap.Reference.Flow
{
    public sealed class ClapFlow: IPuttable<ProcessingEntity>
    {
        private readonly NormalizationSourcing norm;
        private readonly IntermediateStage enrich;
        private readonly IntermediateStage labelling;
        private readonly IntermediateStage predict;
        private readonly IntermediateStage featurecalc;
        private readonly IntermediateStage train;
        private readonly Eventing eventing;

        public RunState State => throw new NotImplementedException();

        private ulong count = 0;

        public void Put(ProcessingEntity item)
        {
            if (item!=null)
            {
                Interlocked.Increment(ref count);
            }
        }

        internal void ExitRecord()
        {
            Interlocked.Decrement(ref count);
        }

        public ClapFlow()
        {
            this.eventing = new Eventing(this);
            this.train = new IntermediateStage(this, eventing);
            this.featurecalc = new IntermediateStage(this, train);
            this.predict = new IntermediateStage(this, featurecalc);
            this.labelling = new IntermediateStage(this, predict);
            this.enrich = new IntermediateStage(this, labelling);
            this.norm = new NormalizationSourcing(this, enrich);
        }
    }

    public abstract class ClapStage
    {
        protected readonly object _lock = new object();
        private readonly ClapStage? nextStage;
        private protected ClapFlowProcessorBase? head;
        private protected ClapFlowProcessorBase? tail;
        private readonly ClapFlow flow;

        internal ClapStage? PreviousStage { get; set; }

        //Scenarios when adding:
        //
        //  nextStage==null  (we're eventing)
        //      we always have a head/tail - starts as a null terminator, so we're ok
        //  PreviousStage==null (we're Normalization)
        //      doesn't affect add per se - but we have an ordering queue to deal with
        //  head==tail==null
        //      this is the first processor, so we need to fixup...
        //      this.PreviousStage.tail.next == this.nextStage.head (we get inserted between them)
        //  tail!=null
        //      we're already set for the add, but may need to deal with:
        //      nextStage.head==null, -> nextStage.nextStage.head==null, etc.
        public virtual void Add(ClapFlowProcessorBase prov)
        {
            if (prov!=null)
            {
                lock(_lock)
                {
                    if (tail!=null) //prior is already pointing at head, just setup next
                    {
                        ClapFlowProcessorBase tmp = tail;
                        tail = prov;
                        tmp.Next = prov;
                        prov.Next = FindNext(); //may be null in theory - but generally shouldn't be (end up with null terminator)
                    }
                    else //we need to find both a prev and next
                    {
                        head = prov;
                        tail = prov;
                        prov.Next = FindNext();
                        ClapFlowProcessorBase? prev = FindPrior();
                        if (prev != null)
                            prev.Next = prov;
                    }
                }
            }
        }

        private ClapFlowProcessorBase? FindPrior()
        {
            if (PreviousStage!=null)
            {
                if (PreviousStage.tail != null)
                    return PreviousStage.tail;
                
                return PreviousStage.FindPrior();
            }
            return null;
        }

        private ClapFlowProcessorBase? FindNext()
        {
            if (nextStage!=null)
            {
                if (nextStage.head != null)
                    return nextStage.head;
                return nextStage.FindNext();
            }
            return null;
        }

        internal ClapStage(ClapFlow flow, ClapStage? nextStage) 
        {
            this.flow = flow;
            this.nextStage = nextStage;
            if (nextStage!=null)
                nextStage.PreviousStage = this;
        }
    }

    public sealed class IntermediateStage : ClapStage
    {
        internal IntermediateStage(ClapFlow flow, ClapStage next):base(flow, next) { }
    }

    public sealed class Eventing : ClapStage 
    {
        private readonly ClapFlowTerminator terminal;

        public override void Add(ClapFlowProcessorBase prov)
        {
            if (prov != null)
            {
                lock (_lock)
                {
                    if (head != terminal && tail!=null)
                    {
                        prov.Next = terminal;
                        tail.Next = prov;
                        tail = prov;
                    }
                    else //we're empty
                    {
                        head = prov;
                        prov.Next = terminal;
                        tail = prov;
                    }
                }
            }
        }

        internal Eventing(ClapFlow flow) : base(flow, null) 
        {
            this.terminal = new ClapFlowTerminator();
            this.head = terminal;
            this.tail = terminal;
        }
    }

    public sealed class NormalizationSourcing : ClapStage
    {
        public override void Add(ClapFlowProcessorBase prov)
        {
            if (prov!=null)
            {
                lock(_lock)
                {
                    if (head != null)
                        prov.Next = head;
                    else
                        prov.Next = tail;
                    
                    head = prov;
                }
            }
        }

        internal NormalizationSourcing(ClapFlow flow, IntermediateStage next):base(flow, next) 
        {
            //tail = ordering queue
        }
    }
}
