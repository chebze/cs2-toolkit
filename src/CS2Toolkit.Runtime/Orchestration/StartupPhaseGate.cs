namespace CS2Toolkit.Runtime.Orchestration;

internal sealed class StartupPhaseGate : Abstractions.IStartupPhase
{
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public StartupPhaseGate(Abstractions.StartupPhase phase) => Phase = phase;

    public Abstractions.StartupPhase Phase { get; }

    public bool IsComplete { get; private set; }

    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        if (IsComplete)
            return Task.CompletedTask;

        return cancellationToken.CanBeCanceled
            ? _completion.Task.WaitAsync(cancellationToken)
            : _completion.Task;
    }

    public void Complete()
    {
        if (IsComplete)
            return;

        IsComplete = true;
        _completion.TrySetResult();
    }

    public void Fail(Exception exception) => _completion.TrySetException(exception);
}
