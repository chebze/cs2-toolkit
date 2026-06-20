using System.Windows.Forms;
using CS2Toolkit.Input.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Input;

public sealed class Win32InputListener : BackgroundService, IInputListener, IInputState
{
    private readonly ILogger<Win32InputListener> _logger;
    private readonly HashSet<int> _heldKeys = new();
    private readonly HashSet<MouseButtons> _heldMouseButtons = new();
    private MouseButton _pressedMouseButtons;

    public Win32InputListener(ILogger<Win32InputListener> logger) => _logger = logger;

    public event EventHandler<KeyInputEvent>? KeyDown;
    public event EventHandler<KeyInputEvent>? KeyUp;
    public event EventHandler<KeyInputEvent>? KeyPress;
    public event EventHandler<MouseInputEvent>? MouseMove;
    public event EventHandler<MouseInputEvent>? MouseDown;

    public IReadOnlyCollection<KeyCode> HeldKeys =>
        _heldKeys.Select(vk => new KeyCode(vk)).ToArray();

    public MouseButton PressedMouseButtons => _pressedMouseButtons;

    public bool IsKeyDown(KeyCode key) => _heldKeys.Contains(key.VirtualKey);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Win32 input listener started");
        var previousMousePosition = Win32InputNative.GetCursorPosition();

        while (!stoppingToken.IsCancellationRequested)
        {
            PollKeyboard();
            PollMouse(ref previousMousePosition);
            await Task.Delay(16, stoppingToken);
        }
    }

    private void PollKeyboard()
    {
        foreach (Keys key in Enum.GetValues<Keys>())
        {
            if (key is Keys.None or Keys.LButton or Keys.RButton or Keys.MButton or Keys.XButton1 or Keys.XButton2)
                continue;

            var virtualKey = (int)key;
            var isDown = Win32InputNative.IsKeyDown(virtualKey);

            if (isDown && _heldKeys.Add(virtualKey))
            {
                var args = new KeyInputEvent { Key = new KeyCode(virtualKey) };
                KeyDown?.Invoke(this, args);
                KeyPress?.Invoke(this, args);
            }
            else if (!isDown && _heldKeys.Remove(virtualKey))
            {
                KeyUp?.Invoke(this, new KeyInputEvent { Key = new KeyCode(virtualKey) });
            }
        }
    }

    private void PollMouse(ref (int X, int Y) previousPosition)
    {
        var position = Win32InputNative.GetCursorPosition();
        if (position != previousPosition)
        {
            MouseMove?.Invoke(this, new MouseInputEvent
            {
                Button = MouseButton.None,
                X = position.X,
                Y = position.Y
            });
            previousPosition = position;
        }

        _pressedMouseButtons = MapMouseButtons(Win32InputNative.IsKeyDown);
        var pressed = ToWinFormsButtons(_pressedMouseButtons);

        foreach (MouseButtons button in Enum.GetValues<MouseButtons>())
        {
            if (button == MouseButtons.None)
                continue;

            var isPressed = pressed.HasFlag(button);
            if (isPressed && _heldMouseButtons.Add(button))
            {
                MouseDown?.Invoke(this, new MouseInputEvent
                {
                    Button = FromWinFormsButton(button),
                    X = position.X,
                    Y = position.Y
                });
            }
            else if (!isPressed)
            {
                _heldMouseButtons.Remove(button);
            }
        }
    }

    private static MouseButton MapMouseButtons(Func<int, bool> isKeyDown)
    {
        var buttons = MouseButton.None;
        if (isKeyDown((int)Keys.LButton)) buttons |= MouseButton.Left;
        if (isKeyDown((int)Keys.RButton)) buttons |= MouseButton.Right;
        if (isKeyDown((int)Keys.MButton)) buttons |= MouseButton.Middle;
        if (isKeyDown((int)Keys.XButton1)) buttons |= MouseButton.X1;
        if (isKeyDown((int)Keys.XButton2)) buttons |= MouseButton.X2;
        return buttons;
    }

    private static MouseButtons ToWinFormsButtons(MouseButton buttons)
    {
        var result = MouseButtons.None;
        if (buttons.HasFlag(MouseButton.Left)) result |= MouseButtons.Left;
        if (buttons.HasFlag(MouseButton.Right)) result |= MouseButtons.Right;
        if (buttons.HasFlag(MouseButton.Middle)) result |= MouseButtons.Middle;
        if (buttons.HasFlag(MouseButton.X1)) result |= MouseButtons.XButton1;
        if (buttons.HasFlag(MouseButton.X2)) result |= MouseButtons.XButton2;
        return result;
    }

    private static MouseButton FromWinFormsButton(MouseButtons button) => button switch
    {
        MouseButtons.Left => MouseButton.Left,
        MouseButtons.Right => MouseButton.Right,
        MouseButtons.Middle => MouseButton.Middle,
        MouseButtons.XButton1 => MouseButton.X1,
        MouseButtons.XButton2 => MouseButton.X2,
        _ => MouseButton.None
    };
}
