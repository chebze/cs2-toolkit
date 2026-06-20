namespace CS2Toolkit.Runtime.Abstractions;

public interface IRuntimeOrchestrator
{
    StartupPhase CurrentPhase { get; }

    bool IsFailed { get; }

    Exception? Failure { get; }

    IStartupPhase GetPhase(StartupPhase phase);

    void CompletePhase(StartupPhase phase);

    Task WaitForPhaseAsync(StartupPhase phase, CancellationToken cancellationToken = default);

    void Fail(Exception exception);

    void RequestShutdown(string reason);
}
