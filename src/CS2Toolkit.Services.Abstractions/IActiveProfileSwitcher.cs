using CS2Toolkit.Configuration.Abstractions;

namespace CS2Toolkit.Services.Abstractions;

public interface IActiveProfileSwitcher
{
    void SwitchTo(string profileId);
    void ApplyActiveProfileToggles(ProfileSettings settings);
}
