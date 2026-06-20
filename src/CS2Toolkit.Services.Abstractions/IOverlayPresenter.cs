using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services.Abstractions;

public interface IOverlayPresenter
{
    string LayerName { get; }
    IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight);
}
