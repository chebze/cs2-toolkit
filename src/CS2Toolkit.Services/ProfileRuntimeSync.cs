using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

internal sealed class ProfileRuntimeSync : IProfileRuntimeSync
{
    private readonly object _gate = new();

    public IDisposable Acquire() => new GateReleaser(_gate);

    private sealed class GateReleaser : IDisposable
    {
        private readonly object _gate;

        public GateReleaser(object gate)
        {
            _gate = gate;
            Monitor.Enter(_gate);
        }

        public void Dispose() => Monitor.Exit(_gate);
    }
}
