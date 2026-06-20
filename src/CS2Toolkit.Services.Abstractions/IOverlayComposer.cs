using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services.Abstractions;

public interface IOverlayComposer
{
    OverlayFrame Compose(GameSnapshot snapshot, int screenWidth, int screenHeight);
}
