using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Linq;

namespace Charon.Vision
{
    /// <summary>
    /// Handles image matching and OCR functionality.
    /// Has option to classify Color or Grayscale images.
    /// </summary>
    public class VisionMatcher : IVisionMatcher, IDisposable
    {
        // DUAL DATABASE: Stores both Color and Gray versions to avoid runtime conversion lag.
        private readonly Dictionary<string, Image<Bgr, byte>> _libraryColor = new Dictionary<string, Image<Bgr, byte>>();
        private readonly Dictionary<string, Image<Gray, byte>> _libraryGray = new Dictionary<string, Image<Gray, byte>>();

        /// <summary>
        /// Loads template images from the specified folder into memory (both Color and Gray versions).
        /// Supports png, jpg, jpeg, bmp.
        /// </summary>
        /// <param name="subFolderPath">Relative path to the folder.</param>
        public void LoadLibrary(string subFolderPath)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subFolderPath);
            if (!Directory.Exists(fullPath)) return;

            var extensions = new[] { ".png", ".jpg", ".jpeg", ".bmp" };
            var files = Directory.GetFiles(fullPath, "*.*")
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()));

            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);

                // Load Color
                var imgColor = new Image<Bgr, byte>(file);
                // Create Gray copy immediately
                var imgGray = imgColor.Convert<Gray, byte>();

                // Store both
                if (_libraryColor.ContainsKey(name))
                {
                    _libraryColor[name].Dispose(); // cleanup old replacement
                    _libraryColor[name] = imgColor;

                    _libraryGray[name].Dispose();
                    _libraryGray[name] = imgGray;
                }
                else
                {
                    _libraryColor.Add(name, imgColor);
                    _libraryGray.Add(name, imgGray);
                }
            }
        }

        // CLASSIFY (COLOR)
        /// <summary>
        /// Finds the best match for the given image from the loaded library (Color).
        /// </summary>
        /// <param name="itemImage">The image to classify.</param>
        /// <param name="threshold">Minimum similarity score.</param>
        /// <returns>Result containing the best match name and score.</returns>
        public MatchResult Classify(Image<Bgr, byte> itemImage, double threshold = 0.85)
        {
            string bestMatchName = "Unknown";
            double bestMatchScore = -1;

            foreach (var kvp in _libraryColor)
            {
                double score = GetMatchScore(itemImage, kvp.Value);
                if (score > bestMatchScore)
                {
                    bestMatchScore = score;
                    bestMatchName = kvp.Key;
                }
            }

            return new MatchResult { 
                Name = bestMatchScore >= threshold ? bestMatchName : "Unknown", 
                Score = bestMatchScore, 
                IsMatch = bestMatchScore >= threshold 
            };
        }

        // CLASSIFY (GRAY)
        /// <summary>
        /// Finds the best match for the given image from the loaded library (Grayscale).
        /// </summary>
        /// <param name="itemImage">The image to classify.</param>
        /// <param name="threshold">Minimum similarity score.</param>
        /// <returns>Result containing the best match name and score.</returns>
        public MatchResult Classify(Image<Gray, byte> itemImage, double threshold = 0.85)
        {
            string bestMatchName = "Unknown";
            double bestMatchScore = -1;

            // Iterate over the PRE-CONVERTED gray library
            foreach (var kvp in _libraryGray)
            {
                double score = GetMatchScore(itemImage, kvp.Value);
                if (score > bestMatchScore)
                {
                    bestMatchScore = score;
                    bestMatchName = kvp.Key;
                }
            }

            return new MatchResult { 
                Name = bestMatchScore >= threshold ? bestMatchName : "Unknown", 
                Score = bestMatchScore, 
                IsMatch = bestMatchScore >= threshold 
            };
        }

        private double GetMatchScore<TColor, TDepth>(Image<TColor, TDepth> input, Image<TColor, TDepth> template)
            where TColor : struct, IColor
            where TDepth : new()
        {
            if (template.Width > input.Width || template.Height > input.Height) return 0.0;

            using (Image<Gray, float> result = input.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
            {
                result.MinMax(out _, out double[] maxValues, out _, out _);
                return maxValues[0];
            }
        }

        public void Dispose()
        {
            foreach (var img in _libraryColor.Values) img.Dispose();
            foreach (var img in _libraryGray.Values) img.Dispose();
            _libraryColor.Clear();
            _libraryGray.Clear();
        }
    }
}