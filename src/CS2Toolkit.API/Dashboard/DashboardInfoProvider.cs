using CS2Toolkit.API.Abstractions;
using CS2Toolkit.Configuration.Abstractions;

namespace CS2Toolkit.API.Dashboard;

internal sealed class DashboardInfoProvider(IConfigurationStore configurationStore) : IDashboardInfoProvider
{
    public DashboardInfo GetDashboardInfo()
    {
        var store = configurationStore.GetStore();
        var active = configurationStore.GetActiveProfile();
        return new DashboardInfo(
            new ActiveProfileSummary(active.Id, active.Name, active.SwitchHotkey),
            store.DefaultProfileId,
            NetworkAccess.GetAccessUrls(store.WebPort),
            store.WebPort,
            "/radar");
    }
}
