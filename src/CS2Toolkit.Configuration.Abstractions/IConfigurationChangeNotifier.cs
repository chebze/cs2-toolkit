namespace CS2Toolkit.Configuration.Abstractions;

public interface IConfigurationChangeNotifier
{
    event Action? ConfigurationChanged;
}
