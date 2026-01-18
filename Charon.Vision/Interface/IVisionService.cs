using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Charon.Vision
{
    public interface IVisionService
    {
        Image<Bgr, byte> CaptureRegion(Rectangle region);
    }
}