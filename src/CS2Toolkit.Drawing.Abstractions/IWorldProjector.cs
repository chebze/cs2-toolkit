using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Drawing.Abstractions;

public interface IWorldProjector
{
    bool TryProject(
        Vector3 world,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        out float screenX,
        out float screenY);
}
