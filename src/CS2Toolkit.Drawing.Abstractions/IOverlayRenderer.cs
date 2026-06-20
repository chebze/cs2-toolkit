using Microsoft.Extensions.Hosting;

namespace CS2Toolkit.Drawing.Abstractions;

public interface IOverlayRenderer : IHostedService
{
    bool IsReady { get; }
}
