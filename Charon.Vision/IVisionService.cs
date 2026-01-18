using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Charon.Vision
{
    public interface IVisionService
    {
        // 1. Update CaptureRegion to include the optional 'useCache' parameter.
        // We set the default to 'false' here too, so the bot knows it's optional.
        Image<Bgr, byte> CaptureRegion(Rectangle region, bool useCache = false);

        // 2. Add CaptureRegionGray (It was missing!)
        Image<Gray, byte> CaptureRegionGray(Rectangle region, bool useCache = false);
    }
}