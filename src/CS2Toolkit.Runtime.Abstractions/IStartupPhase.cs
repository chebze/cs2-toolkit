namespace CS2Toolkit.Runtime.Abstractions;

/// <summary>
/// Gate for a single ordered startup phase. Completed by <see cref="IRuntimeOrchestrator"/>.
/// </summary>
public interface IStartupPhase
{
    StartupPhase Phase { get; }

    bool IsComplete { get; }

    Task WaitAsync(CancellationToken cancellationToken = default);
}
