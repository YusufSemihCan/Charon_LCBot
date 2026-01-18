using System;
using System.Drawing; // Requires System.Drawing.Common
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace Charon.Vision
{
    public class TemplateMatcher
    {
        /// <summary>
        /// Scans the 'screen' for the 'template' image.
        /// </summary>
        /// <param name="screen">The big image (screenshot).</param>
        /// <param name="template">The small image we are looking for (e.g., a button).</param>
        /// <param name="threshold">How strict the match is (0.0 to 1.0). 0.9 is recommended.</param>
        /// <returns>The Rectangle where the object was found, or Rectangle.Empty if not found.</returns>
        public Rectangle Find(Image<Bgr, byte> screen, Image<Bgr, byte> template, double threshold = 0.9)
        {
            // Optimization: If the template is bigger than the screen, it's impossible to match.
            if (template.Width > screen.Width || template.Height > screen.Height)
                return Rectangle.Empty;

            // 1. Create a result matrix to store match probabilities
            // The result image is smaller than the source: (W-w+1) x (H-h+1)
            using (Image<Gray, float> result = screen.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
            {
                // 2. Find the highest value in the result matrix
                // "MinMaxLoc" is an optimized OpenCV function that scans the matrix in C++ (very fast)
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;

                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // For CcoeffNormed, the 'max' value is the best match score (1.0 = perfect match)
                double matchScore = maxValues[0];
                Point matchLocation = maxLocations[0];

                // 3. Decide if it's a good match
                if (matchScore >= threshold)
                {
                    // Return the rectangle of where the object is
                    return new Rectangle(matchLocation, template.Size);
                }
                else
                {
                    // No match found good enough
                    return Rectangle.Empty;
                }
            }
        }
    }
}