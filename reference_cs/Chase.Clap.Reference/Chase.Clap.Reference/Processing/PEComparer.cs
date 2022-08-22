using OsrsOpen.Chase.Reference;

namespace Chase.Clap.Reference.Processing
{
    internal sealed class PEComparer : IComparer<ProcessingEntityInstant>
    {
        public int Compare(ProcessingEntityInstant? x, ProcessingEntityInstant? y)
        {
            if (x != null)
            {
                if (y != null)
                {
                    return x.Timestamp.CompareTo(y.Timestamp);
                }
                else
                    return 1;
            }
            else
            {
                if (y != null)
                    return -1;
                return 0;
            }
        }
    }
}
