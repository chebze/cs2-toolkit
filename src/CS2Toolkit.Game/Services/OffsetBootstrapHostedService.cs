using CS2Toolkit.Game.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Game.Services;

internal sealed class OffsetBootstrapHostedService : IHostedService
{
    private readonly IOffsetProvider _offsetProvider;
    private readonly GameAttachmentService _attachment;
    private readonly ILogger<OffsetBootstrapHostedService> _logger;

    public OffsetBootstrapHostedService(
        IOffsetProvider offsetProvider,
        GameAttachmentService attachment,
        ILogger<OffsetBootstrapHostedService> logger)
    {
        _offsetProvider = offsetProvider;
        _attachment = attachment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _offsetProvider.EnsureLoadedAsync(cancellationToken);
            _attachment.SetState(GameLifecycleState.WaitingForAttach);
            _logger.LogInformation("Offsets loaded — press inject key to attach to CS2");
        }
        catch (Exception ex)
        {
            _attachment.SetState(GameLifecycleState.Failed);
            _logger.LogError(ex, "Failed to load offsets");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
