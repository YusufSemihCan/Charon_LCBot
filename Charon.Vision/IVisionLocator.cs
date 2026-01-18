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
        void IndexTemplates(string subFolderPath);
        void UnloadFromRam(string templateName);
        //  FIND (Fast Gray)
        Rectangle Find(Image<Gray, byte> screen, string templateName, double threshold = 0.9);
        //  FIND (Precise Color)
        Rectangle Find(Image<Bgr, byte> screen, string templateName, double threshold = 0.9);

        // READ (Standard Gray)
        string Read(Image<Gray, byte> screen, Rectangle area);

        // READ (Color Convenience)
        string Read(Image<Bgr, byte> screen, Rectangle area);
    }
}