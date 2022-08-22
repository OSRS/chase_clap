namespace OsrsOpen.Chase.Reference.Processing
{
    public enum RunState
    {
        /// <summary>
        /// The state is unknown in some way or for some reason. Any integer value that does not map to a specific value of this enum should also be treated as Unknown.
        /// This state should, in general, not be used and be considered a critically failed or misbehaving state.
        /// </summary>
        Unknown = int.MinValue,
        /// <summary>
        /// A failure has occurred such that this instance should not be restarted in any way and this should be considered a critically dead state.
        /// </summary>
        FatalFailure = int.MinValue + 1,

        Created = 0,
        Bootstrapping = 1,
        FailedBootstrapping = 2,
        Bootstrapped = 3,
        Initializing = 4,
        FailedInitializing = 5,
        Initialized = 6,
        Starting = 7,
        FailedStarting = 8,
        Running = 9,
        FailedRunning = 10,
        /// <summary>
        /// This state indicates a transition from running to paused while the transition is in progress. If the transition fails, the state should transition to "FailedStopping" and the next transition should be to attempt "Start" or "Stop".
        /// </summary>
        Pausing = 11,
        /// <summary>
        /// This state indicates the instance is currently paused. The next transition should be to "Resume", or "Stop".
        /// </summary>
        Paused = 12,
        /// <summary>
        /// This state indicates a transition from "Paused" to "Running" while the transition is in progress. If the transition fails, the state should transition to "FailedStarting" and the next transition should be to attempt "Start" or "Stop".
        /// </summary>
        Resuming = 13,
        Stopping = 14,
        FailedStopping = 15,
        Stopped = 16,
        Uninitializing = 17,
        FailedUninitializing = 18,
        Uninitialized = 19,
        UnBootstrapping = 20,
        FailedUnbootstrapping = 21,
        UnBootstrapped = 22,
        /// <summary>
        /// Same as concept of "ran to completion" - implies should not be started again but did not fail
        /// </summary>
        Shutdown = 23
    }
}
