using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Charon.Vision
{
    public interface IVisionService
    {
        Rectangle ScreenResolution { get; }

        // 1. Update CaptureRegion to include the optional 'useCache' parameter.
        // We set the default to 'false' here too, so the bot knows it's optional.
        /// <summary>
        /// Captures a specific region of the screen.
        /// </summary>
        Image<Bgr, byte> CaptureRegion(Rectangle region, bool useCache = false);

        /// <summary>
        /// Captures the entire primary screen.
        /// </summary>
        Image<Bgr, byte> CaptureScreen(bool useCache = false);

        /// <summary>
        /// Captures a specific region of the screen as Grayscale.
        /// </summary>
        Image<Gray, byte> CaptureRegionGray(Rectangle region, bool useCache = false);
    }
}