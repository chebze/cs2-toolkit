namespace CS2Toolkit.Configuration.Abstractions;

public interface IConfigurationStore
{
    ConfigurationStore GetStore();
    ConfigProfile GetActiveProfile();
    ConfigProfile? GetProfile(string id);
    void SetActiveProfile(string profileId);
    void SetDefaultProfile(string profileId);
    ConfigProfile CreateProfile(string name);
    ConfigProfile UpdateProfile(ConfigProfile profile);
    void DeleteProfile(string profileId);
    void UpdateKeybinds(GlobalKeybinds keybinds);
    void UpdateWebPort(int port);
    string ExportProfile(string profileId);
    ConfigProfile ImportProfile(string json, string? nameOverride = null);
}
