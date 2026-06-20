using CS2Toolkit.Runtime.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Runtime.Orchestration;

internal sealed class RuntimeOrchestrator : IRuntimeOrchestrator
{
    private static readonly StartupPhase[] PhaseOrder =
    [
        StartupPhase.Offsets,
        StartupPhase.Maps,
        StartupPhase.Overlay,
        StartupPhase.Input,
        StartupPhase.Attach,
        StartupPhase.GameLoop,
        StartupPhase.Features,
        StartupPhase.Api
    ];

    private readonly Dictionary<StartupPhase, StartupPhaseGate> _gates =
        PhaseOrder.ToDictionary(phase => phase, phase => new StartupPhaseGate(phase));

    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<RuntimeOrchestrator> _logger;
    private readonly object _lock = new();

    public RuntimeOrchestrator(
        IHostApplicationLifetime lifetime,
        ILogger<RuntimeOrchestrator> logger)
    {
        _lifetime = lifetime;
        _logger = logger;
    }

    public StartupPhase CurrentPhase { get; private set; } = StartupPhase.Offsets;

    public bool IsFailed { get; private set; }

    public Exception? Failure { get; private set; }

    public IStartupPhase GetPhase(StartupPhase phase) => _gates[phase];

    public void CompletePhase(StartupPhase phase)
    {
        lock (_lock)
        {
            if (IsFailed)
                return;

            var index = Array.IndexOf(PhaseOrder, phase);
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(phase), phase, "Unknown startup phase.");

            CurrentPhase = phase;
            _gates[phase].Complete();
            _logger.LogInformation("Startup phase complete: {Phase}", phase);
        }
    }

    public Task WaitForPhaseAsync(StartupPhase phase, CancellationToken cancellationToken = default) =>
        _gates[phase].WaitAsync(cancellationToken);

    public void Fail(Exception exception)
    {
        lock (_lock)
        {
            if (IsFailed)
                return;

            IsFailed = true;
            Failure = exception;
            _logger.LogCritical(exception, "Runtime orchestration failed at phase {Phase}", CurrentPhase);

            foreach (var gate in _gates.Values)
                gate.Fail(exception);
        }
    }

    public void RequestShutdown(string reason)
    {
        _logger.LogWarning("Shutdown requested: {Reason}", reason);
        _lifetime.StopApplication();
    }
}
