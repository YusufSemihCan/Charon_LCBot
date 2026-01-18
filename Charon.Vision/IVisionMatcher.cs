using Emgu.CV;
using Emgu.CV.Structure;

namespace Charon.Vision
{
    // A lightweight result packet. 
    // Logic layer uses this to decide what to do next.
    public struct MatchResult
    {
        public string Name;      // e.g., "Ruby", "TrashJunk", "EmptySlot"
        public double Score;     // 0.0 to 1.0 (Higher is better)
        public bool IsMatch;     // True if Score > Threshold

        // Helper for when nothing is found
        public static MatchResult Empty => new MatchResult { Name = "Unknown", Score = 0, IsMatch = false };
    }

    public interface IVisionMatcher
    {
        // Loads the database of known items (e.g. "Assets/Items")
        void LoadLibrary(string subFolderPath);

        // CLASSIFY: Compares the input image against the entire library
        // Returns the single best match found.
        MatchResult Classify(Image<Bgr, byte> itemImage, double threshold = 0.85);
        MatchResult Classify(Image<Gray, byte> itemImage, double threshold = 0.85);
    }
}