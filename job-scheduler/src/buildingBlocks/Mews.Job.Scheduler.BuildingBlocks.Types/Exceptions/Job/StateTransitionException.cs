namespace Mews.Job.Scheduler.Job;

public sealed class StateTransitionException : Exception
{
    public StateTransitionException(string fromState, string toState)
        : base($"Invalid state transition from {fromState} to {toState}.")
    {
        FromState = fromState;
        ToState = toState;
    }

    /// <summary>
    /// State from which the transition was attempted
    /// </summary>
    public string FromState { get; }

    /// <summary>
    /// State to which the transition was attempted
    /// </summary>
    public string ToState { get; }
}
