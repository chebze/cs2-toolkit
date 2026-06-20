using Cs2Toolkit.Models;

namespace Cs2Toolkit.Events;

public sealed class ToolkitEventBus
{
    public event EventHandler<KeyInputEventArgs>? OnKeyDown;
    public event EventHandler<KeyInputEventArgs>? OnKeyUp;
    public event EventHandler<KeyInputEventArgs>? OnKeyPress;
    public event EventHandler<MouseInputEventArgs>? OnMousePress;
    public event EventHandler<MouseInputEventArgs>? OnMouseMove;
    public event EventHandler<MemoryReadEventArgs>? OnMemoryRead;
    public event EventHandler<InjectionStatusEventArgs>? OnInjectionStatusChanged;

    public void PublishKeyDown(KeyInputEventArgs args) => OnKeyDown?.Invoke(this, args);
    public void PublishKeyUp(KeyInputEventArgs args) => OnKeyUp?.Invoke(this, args);
    public void PublishKeyPress(KeyInputEventArgs args) => OnKeyPress?.Invoke(this, args);
    public void PublishMousePress(MouseInputEventArgs args) => OnMousePress?.Invoke(this, args);
    public void PublishMouseMove(MouseInputEventArgs args) => OnMouseMove?.Invoke(this, args);
    public void PublishMemoryRead(MemoryState state) =>
        OnMemoryRead?.Invoke(this, new MemoryReadEventArgs { State = state });
    public void PublishInjectionStatus(InjectionStatus status, string message) =>
        OnInjectionStatusChanged?.Invoke(this, new InjectionStatusEventArgs { Status = status, Message = message });
}
