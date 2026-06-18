namespace Cs2Toolkit.Runtime;

public sealed class RuntimeGate
{
    private readonly TaskCompletionSource _overlayReadyTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _mapParsingCompleteTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _injectionCompleteTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task OverlayReadyTask => _overlayReadyTcs.Task;
    public Task MapParsingCompleteTask => _mapParsingCompleteTcs.Task;
    public Task InjectionCompleteTask => _injectionCompleteTcs.Task;
    public Task MemoryReaderStartTask => Task.WhenAll(_overlayReadyTcs.Task, _injectionCompleteTcs.Task);

    public bool IsOverlayReady { get; private set; }
    public bool IsMapParsingComplete { get; private set; }
    public bool IsInjectionComplete { get; private set; }

    public void SignalOverlayReady()
    {
        if (IsOverlayReady) return;
        IsOverlayReady = true;
        _overlayReadyTcs.TrySetResult();
    }

    public void SignalMapParsingComplete()
    {
        if (IsMapParsingComplete) return;
        IsMapParsingComplete = true;
        _mapParsingCompleteTcs.TrySetResult();
    }

    public void SignalInjectionComplete()
    {
        if (IsInjectionComplete) return;
        IsInjectionComplete = true;
        _injectionCompleteTcs.TrySetResult();
    }
}
