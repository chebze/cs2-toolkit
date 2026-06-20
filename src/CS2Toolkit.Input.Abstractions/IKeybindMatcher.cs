namespace CS2Toolkit.Input.Abstractions;

public sealed record KeybindMatch(string ActionId, string KeyName);

public interface IKeybindMatcher
{
    KeyCode ParseKey(string keyName);
    bool TryMatchKeyDown(KeyInputEvent input, out KeybindMatch match);
    IReadOnlyList<KeybindDefinition> GetKeybinds();
}
