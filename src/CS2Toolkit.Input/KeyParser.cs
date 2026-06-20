using System.Windows.Forms;
using CS2Toolkit.Input.Abstractions;

namespace CS2Toolkit.Input;

public static class KeyParser
{
    public static KeyCode Parse(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName))
            return KeyCode.None;

        return Enum.TryParse<Keys>(keyName, ignoreCase: true, out var key)
            ? new KeyCode((int)key)
            : KeyCode.None;
    }

    public static string ToKeyName(KeyCode key)
    {
        if (key.IsNone)
            return string.Empty;

        return Enum.IsDefined(typeof(Keys), key.VirtualKey)
            ? ((Keys)key.VirtualKey).ToString()
            : key.VirtualKey.ToString();
    }
}
