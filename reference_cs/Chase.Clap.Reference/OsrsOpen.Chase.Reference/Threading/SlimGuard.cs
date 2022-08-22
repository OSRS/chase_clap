using System.Diagnostics;

namespace OsrsOpen.Chase.Reference.Threading
{
    /// <summary>
    /// A slim, non-blocking (CAS) guard for use as a mutex.
    /// Note that is potentially very unsafe if used improperly.
    /// 
    /// Single try usage:
    /// 
    /// SlimGuard g = new SlimGuard();
    /// if (g.Enter)
    /// {
    ///    //Do interesting work
    ///    
    ///    g.Exit();
    /// }
    /// 
    /// Retrying usage:
    /// 
    /// while(!g.Enter)
    /// {
    ///   //Do other work
    /// }
    /// 
    /// //Do interesting work
    /// 
    /// g.Exit();
    /// 
    /// </summary>
    public sealed class SlimGuard
    {
        private int waiter;

        public bool Enter
        {
            [DebuggerStepThrough]
            get
            {
                return 0 == Interlocked.Exchange(ref waiter, 1);
            }
        }

        [DebuggerStepThrough]
        public void Exit()
        {
            Interlocked.Exchange(ref waiter, 0);
        }
    }
}
