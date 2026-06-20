namespace CS2Toolkit.Services.Abstractions;

public interface IFeatureRegistry
{
    IReadOnlyList<IFeatureService> Features { get; }
    bool TryGet(FeatureId id, out IFeatureService? feature);
    bool IsEnabled(FeatureId id);
    void SetEnabled(FeatureId id, bool enabled);
    bool Toggle(FeatureId id);
}
