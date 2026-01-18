using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace Charon.Vision
{
    public class VisionMatcher : IVisionMatcher, IDisposable
    {
        // DUAL DATABASE: Stores both Color and Gray versions to avoid runtime conversion lag.
        private readonly Dictionary<string, Image<Bgr, byte>> _libraryColor = new Dictionary<string, Image<Bgr, byte>>();
        private readonly Dictionary<string, Image<Gray, byte>> _libraryGray = new Dictionary<string, Image<Gray, byte>>();

        public void LoadLibrary(string subFolderPath)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subFolderPath);
            if (!Directory.Exists(fullPath)) return;

            foreach (var file in Directory.GetFiles(fullPath, "*.png"))
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

            return new MatchResult { Name = bestMatchName, Score = bestMatchScore, IsMatch = bestMatchScore >= threshold };
        }

        // CLASSIFY (GRAY) - NOW BLAZING FAST
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

            return new MatchResult { Name = bestMatchName, Score = bestMatchScore, IsMatch = bestMatchScore >= threshold };
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