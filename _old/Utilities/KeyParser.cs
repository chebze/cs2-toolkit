namespace Cs2Toolkit.Utilities;

public static class KeyParser
{
    public static Keys Parse(string keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName))
            return Keys.None;

        return Enum.TryParse<Keys>(keyName, ignoreCase: true, out var key)
            ? key
            : Keys.None;
    }
}
