namespace CS2Toolkit.Runtime.Abstractions;

/// <summary>
/// Optional startup module hook for future plugins.
/// </summary>
public interface IToolkitModule
{
    string Name { get; }

    int Order { get; }

    Task InitializeAsync(IRuntimeOrchestrator orchestrator, CancellationToken cancellationToken);
}
