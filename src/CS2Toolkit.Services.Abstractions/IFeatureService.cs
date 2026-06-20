namespace CS2Toolkit.Services.Abstractions;

public interface IFeatureService
{
    FeatureId Id { get; }
    bool IsEnabled { get; }
    void OnSnapshot(FeatureContext context);
}
