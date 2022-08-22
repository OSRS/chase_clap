using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chase.Clap.Reference.Processing
{
    internal sealed class OrderedLinkedList<T>
    {
        private LinkedList<T> items = new LinkedList<T>();

        private int count = 0;
        public int Count { get { return count; } }

        public T? Remove()
        {
            if (items.Last!=null)
            {
                T tmp = items.Last.Value;
                Interlocked.Decrement(ref count);
                items.RemoveLast();
                return tmp;
            }
            return default;
        }

        //NOTE: single threaded access only
        public void Add(T item)
        {
            if (items.First!=null)
            {
                LinkedListNode<T> nod = items.First;
                int ix = 0;
                while (0 > comp.Compare(item, nod.Value))
                {
                    if (nod.Next != null)
                    {
                        nod = nod.Next;
                        ix++;
                        if (ix >= count)
                        {
                            items.AddLast(item);
                            Interlocked.Increment(ref count);
                            return;
                        }
                    }
                    else
                    {
                        items.AddLast(item);
                        Interlocked.Increment(ref count);
                        return;
                    }
                }

                items.AddBefore(nod, item);
                Interlocked.Increment(ref count);
            }
            else
            {
                items.AddLast(item);
                Interlocked.Increment(ref count);
            }
        }

        private readonly IComparer<T> comp;

        public OrderedLinkedList(IComparer<T> comp)
        {
            this.comp = comp;
        }
    }
}
