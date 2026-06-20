using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Drawing.Abstractions;

public sealed class WorldProjector : IWorldProjector
{
    public bool TryProject(
        Vector3 world,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        out float screenX,
        out float screenY)
    {
        screenX = 0;
        screenY = 0;

        var matrix = viewMatrix.Values;
        if (matrix.Length < ViewMatrix.FloatCount || screenWidth <= 0 || screenHeight <= 0 || !world.IsValid)
            return false;

        var w = matrix[12] * world.X + matrix[13] * world.Y + matrix[14] * world.Z + matrix[15];
        if (w < 0.001f)
            return false;

        var clipX = matrix[0] * world.X + matrix[1] * world.Y + matrix[2] * world.Z + matrix[3];
        var clipY = matrix[4] * world.X + matrix[5] * world.Y + matrix[6] * world.Z + matrix[7];

        var invW = 1f / w;
        clipX *= invW;
        clipY *= invW;

        screenX = screenWidth * 0.5f + clipX * screenWidth * 0.5f;
        screenY = screenHeight * 0.5f - clipY * screenHeight * 0.5f;

        if (float.IsNaN(screenX) || float.IsNaN(screenY) || float.IsInfinity(screenX) || float.IsInfinity(screenY))
            return false;

        return screenX >= -screenWidth && screenX <= screenWidth * 2f
            && screenY >= -screenHeight && screenY <= screenHeight * 2f;
    }
}
