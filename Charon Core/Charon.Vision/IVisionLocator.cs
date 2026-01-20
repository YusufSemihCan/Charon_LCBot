using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Charon.Vision
{
    // Define the CacheMode options here so they are visible to the Interface
    public enum CacheMode
    {
        Speed,      // Preload ALL images to RAM (High RAM, Instant Search)
        Memory,     // Never cache (Low RAM, Slower Search)
        Balanced    // Smart Cache (Maintains a fixed number of items)
    }

    public interface IVisionLocator
    {
        /// <summary>
        /// Global scaling factor applied to all loaded templates (e.g. 0.66 for 720p).
        /// </summary>
        double ScaleFactor { get; set; }

        void IndexTemplates(string subFolderPath);
        void UnloadFromRam(string templateName);
        //  FIND (Fast Gray)
        /// <summary>
        /// Finds the specified template within the screen image using Grayscale matching.
        /// </summary>
        Rectangle Find(Image<Gray, byte> screen, string templateName, double threshold = 0.9, bool useEdges = false);

        /// <summary>
        /// Finds the specified template within the screen image using Color matching.
        /// </summary>
        Rectangle Find(Image<Bgr, byte> screen, string templateName, double threshold = 0.9, bool useEdges = false);

        /// <summary>
        /// Performs OCR on a specific region of a Grayscale image.
        /// </summary>
        string Read(Image<Gray, byte> screen, Rectangle area);

        /// <summary>
        /// Performs OCR on a specific region of a Color image (automatically converts to Gray).
        /// </summary>
        string Read(Image<Bgr, byte> screen, Rectangle area);
    }
}