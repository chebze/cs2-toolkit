using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services.Abstractions;

public interface IStatusToastPublisher
{
    bool HasActive { get; }

    IReadOnlyList<StatusToast> GetActive();

    void Publish(string message, TimeSpan? duration = null, uint colorArgb = 0);

    void SetPersistent(string message, uint colorArgb = 0);

    void Clear();

    void ClearPersistent();
}
