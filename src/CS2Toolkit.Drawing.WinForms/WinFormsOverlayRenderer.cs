using CS2Toolkit.Drawing.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Drawing.WinForms;

public sealed class WinFormsOverlayRenderer : IOverlayRenderer
{
    private readonly IOverlayFrameSource _frameSource;
    private readonly WinFormsOverlayHost _host = new();
    private readonly ILogger<WinFormsOverlayRenderer> _logger;

    private readonly System.Windows.Forms.Timer _renderTimer;

    private Thread? _uiThread;
    private OverlayForm? _form;
    private int _topMostRefreshTicks;
    private long _lastConsumedSequence = -1;

    public bool IsReady { get; private set; }

    public WinFormsOverlayRenderer(
        IOverlayFrameSource frameSource,
        ILogger<WinFormsOverlayRenderer> logger)
    {
        _frameSource = frameSource;
        _logger = logger;

        _renderTimer = new System.Windows.Forms.Timer { Interval = 1 };
        _renderTimer.Tick += OnRenderTick;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _uiThread = new Thread(() =>
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _form = new OverlayForm(_host);
            _renderTimer.Start();
            IsReady = true;
            _logger.LogInformation("WinForms overlay renderer ready (uncapped present rate)");
            tcs.TrySetResult();

            Application.Run(_form);
        })
        {
            IsBackground = true,
            Name = "OverlayUIThread"
        };

        _uiThread.SetApartmentState(ApartmentState.STA);
        _uiThread.Start();

        return tcs.Task.WaitAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _renderTimer.Stop();

        if (_form is not null && !_form.IsDisposed)
        {
            if (_form.InvokeRequired)
                _form.BeginInvoke(_form.Close);
            else
                _form.Close();
        }

        return Task.CompletedTask;
    }

    private void OnRenderTick(object? sender, EventArgs e)
    {
        if (_form is null || _form.IsDisposed)
            return;

        if (!_frameSource.TryGetLatest(out var frame) || frame.Sequence == _lastConsumedSequence)
            return;

        _lastConsumedSequence = frame.Sequence;
        _form.SetClickThrough(!frame.Interactive);
        _form.PresentFrame(frame);

        _topMostRefreshTicks++;
        if (_topMostRefreshTicks >= 120)
        {
            _topMostRefreshTicks = 0;
            _form.EnsureOnTop();
        }
    }
}
