using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using Tesseract;
using System.Linq;

namespace Charon.Vision
{
    /// <summary>
    /// Handles finding images (Pattern Matching) and reading text (OCR).
    /// Manages its own memory using a Cache system (Speed vs Memory vs Balanced).
    /// </summary>
    public class VisionLocator : IVisionLocator, IDisposable
    {
        // SETTINGS
        private readonly CacheMode _mode;       // Speed (RAM heavy), Memory (HDD heavy), or Balanced (Smart)
        private readonly int _maxCacheSize;     // How many images to keep in RAM (Balanced mode only)

        // STORAGE
        private readonly Dictionary<string, string> _filePaths = new Dictionary<string, string>(); // The "Phonebook" (Path to file)
        private readonly Dictionary<string, Image<Gray, byte>> _grayCache = new Dictionary<string, Image<Gray, byte>>(); // RAM Storage (Gray)
        private readonly Dictionary<string, Image<Bgr, byte>> _colorCache = new Dictionary<string, Image<Bgr, byte>>(); // RAM Storage (Color)
        private readonly List<string> _lruTracker = new List<string>(); // Tracks usage order for the cache

        // ENGINES
        private readonly TesseractEngine? _ocrEngine;

        public VisionLocator(CacheMode mode = CacheMode.Balanced, int maxCacheSize = 20)
        {
            _mode = mode;
            _maxCacheSize = maxCacheSize;
            try
            {
                // Initialize Tesseract (OCR)
                // We assume 'tessdata' folder is in the same folder as the .exe
                string tessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
                _ocrEngine = new TesseractEngine(tessPath, "eng", EngineMode.Default);
            }
            catch
            {
                // If OCR fails to load (missing files), we don't crash the app.
                // Text functions will simply return empty strings.
                _ocrEngine = null;
            }
        }

        // =========================================================
        //  1. SETUP & INDEXING
        // =========================================================

        /// <summary>
        /// Scans the specified folder for template images (png, jpg, jpeg, bmp) and indexes them.
        /// </summary>
        /// <param name="subFolderPath">Relative path to the folder containing template images.</param>
        public void IndexTemplates(string subFolderPath)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subFolderPath);
            if (!Directory.Exists(fullPath)) return;

            // Scan the folder for supported image formats using AllDirectories to support subfolders
            var extensions = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" };
            var files = extensions.SelectMany(ext => Directory.GetFiles(fullPath, ext, SearchOption.AllDirectories)).Distinct();

            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);

                // Add to our "Phonebook" so we know where to find it later
                if (!_filePaths.ContainsKey(name))
                {
                    _filePaths.Add(name, file);

                    // OPTIMIZATION: If user chose "Speed" mode, load everything into RAM now.
                    // We default to Gray as it's the most common use case.
                    if (_mode == CacheMode.Speed)
                    {
                        AddToCache(name, new Image<Gray, byte>(file));
                    }
                }
            }
        }

        // =========================================================
        //  2. FIND FUNCTIONS (PATTERN MATCHING)
        // =========================================================

        // [GRAYSCALE SEARCH] - Fast, Robust, Low RAM
        // [GRAYSCALE SEARCH] - Fast, Robust, Low RAM
        /// <summary>
        /// Finds the specified template within the screen image using Grayscale matching.
        /// </summary>
        /// <param name="screen">The screen image to search within.</param>
        /// <param name="templateName">The name of the template to find (filename without extension).</param>
        /// <param name="threshold">Minimum similarity score (0.0 to 1.0) to consider a match.</param>
        /// <param name="useEdges">If true, uses Canny edge detection for matching (slower but handles lighting/color variations better).</param>
        /// <returns>The bounding rectangle of the found template, or Rectangle.Empty if not found.</returns>
        public Rectangle Find(Image<Gray, byte> screen, string templateName, double threshold = 0.9, bool useEdges = false)
        {
            if (!_filePaths.ContainsKey(templateName)) return Rectangle.Empty;

            // A. CHECK RAM (Fastest)


            // A. CHECK RAM (Fastest)
            if (_grayCache.ContainsKey(templateName))
            {
                UpdateLru(templateName); // Mark as "Recently Used"
                return PerformMatch(screen, _grayCache[templateName], threshold, useEdges);
            }

            // B. LOAD FROM DISK (Slower)
            string path = _filePaths[templateName];
            Image<Gray, byte> loadedImg = new Image<Gray, byte>(path);

            // C. CACHE LOGIC
            if (_mode == CacheMode.Memory)
            {
                // Memory Mode: Use once, then destroy immediately.
                using (loadedImg) return PerformMatch(screen, loadedImg, threshold, useEdges);
            }
            else
            {
                // Balanced/Speed Mode: Save to RAM (handling eviction if full)
                AddToCache(templateName, loadedImg);
                return PerformMatch(screen, loadedImg, threshold, useEdges);
            }
        }

        // [COLOR SEARCH] - Precise (Red vs Blue), Higher RAM
        // [COLOR SEARCH] - Precise (Red vs Blue), Higher RAM
        /// <summary>
        /// Finds the specified template within the screen image using Color matching.
        /// </summary>
        /// <param name="screen">The screen image to search within.</param>
        /// <param name="templateName">The name of the template to find.</param>
        /// <param name="threshold">Minimum similarity score.</param>
        /// <param name="useEdges">if true, uses edge detection (converts to gray internally).</param>
        /// <returns>Rectangle of match.</returns>
        public Rectangle Find(Image<Bgr, byte> screen, string templateName, double threshold = 0.9, bool useEdges = false)
        {
            if (!_filePaths.ContainsKey(templateName)) return Rectangle.Empty;

            // A. CHECK RAM
            if (_colorCache.ContainsKey(templateName))
            {
                UpdateLru(templateName);
                return PerformMatch(screen, _colorCache[templateName], threshold, useEdges);
            }

            // B. LOAD FROM DISK
            string path = _filePaths[templateName];
            Image<Bgr, byte> loadedImg = new Image<Bgr, byte>(path);

            // C. CACHE LOGIC
            if (_mode == CacheMode.Memory)
            {
                using (loadedImg) return PerformMatch(screen, loadedImg, threshold, useEdges);
            }
            else
            {
                AddToCache(templateName, loadedImg);
                return PerformMatch(screen, loadedImg, threshold, useEdges);
            }
        }

        // =========================================================
        //  3. READ FUNCTIONS (OCR / TEXT)
        // =========================================================

        // [STRICT READ] - Expects Gray (Performance safe)
        // [STRICT READ] - Expects Gray (Performance safe)
        /// <summary>
        /// Performs OCR on a specific region of a Grayscale image.
        /// </summary>
        /// <param name="screen">The image to read from.</param>
        /// <param name="area">The region to crop and read.</param>
        /// <returns>The recognized text.</returns>
        public string Read(Image<Gray, byte> screen, Rectangle area)
        {
            if (_ocrEngine == null) return "";

            screen.ROI = area;
            try
            {
                using (var cropped = screen.Copy())
                using (var binary = cropped.ThresholdBinary(new Gray(150), new Gray(255)))
                using (var bmp = binary.ToBitmap())
                {
                    // Try passing the Bitmap directly; most modern Tesseract wrappers 
                    // handle the conversion internally now.
                    using (var page = _ocrEngine.Process(bmp))
                    {
                        return page.GetText().Trim();
                    }
                }
            }
            finally
            {
                screen.ROI = Rectangle.Empty; // CRITICAL: Always reset ROI
            }
        }

        // [CONVENIENCE READ] - Accepts Color, converts to Gray automatically.
        // [CONVENIENCE READ] - Accepts Color, converts to Gray automatically.
        /// <summary>
        /// Performs OCR on a specific region of a Color image (automatically converts to Gray).
        /// </summary>
        public string Read(Image<Bgr, byte> screen, Rectangle area)
        {
            // Tesseract works poorly with Color.
            // We force a conversion here so the Bot Logic doesn't have to handle it manually.
            using (var grayScreen = screen.Convert<Gray, byte>())
            {
                return Read(grayScreen, area);
            }
        }

        // =========================================================
        //  4. MEMORY MANAGEMENT (CACHE & DISPOSE)


        private void AddToCache<TColor, TDepth>(string name, Image<TColor, TDepth> img)
            where TColor : struct, IColor
            where TDepth : new()
        {
            // If we are in "Balanced" mode and the cache is full...
            if (_mode == CacheMode.Balanced && _lruTracker.Count >= _maxCacheSize)
            {
                // ...delete the item that hasn't been used in the longest time.
                string oldestKey = _lruTracker[0];
                UnloadFromRam(oldestKey);
            }

            // Store the new image
            if (img is Image<Gray, byte> gray) _grayCache[name] = gray;
            if (img is Image<Bgr, byte> color) _colorCache[name] = color;

            UpdateLru(name);
        }

        private void UpdateLru(string name)
        {
            // Move item to the end of the list (Mark as "Newest")
            if (_lruTracker.Contains(name)) _lruTracker.Remove(name);
            _lruTracker.Add(name);
        }

        public void UnloadFromRam(string templateName)
        {
            // Dispose and remove from Gray Cache
            if (_grayCache.ContainsKey(templateName))
            {
                _grayCache[templateName].Dispose();
                _grayCache.Remove(templateName);
            }
            // Dispose and remove from Color Cache
            if (_colorCache.ContainsKey(templateName))
            {
                _colorCache[templateName].Dispose();
                _colorCache.Remove(templateName);
            }
            // Remove from tracker
            if (_lruTracker.Contains(templateName)) _lruTracker.Remove(templateName);
        }

        // The "Math" behind finding an image
        private Rectangle PerformMatch<TColor, TDepth>(Image<TColor, TDepth> screen, Image<TColor, TDepth> template, double threshold, bool useEdges = false)
            where TColor : struct, IColor
            where TDepth : new()
        {
            if (template.Width > screen.Width || template.Height > screen.Height) return Rectangle.Empty;
            
            // FIX: Edge detection helps find "smooth" objects by matching outlines
            if (useEdges)
            {
                using (var screenGray = screen.Convert<Gray, byte>())
                using (var templateGray = template.Convert<Gray, byte>())
                using (var screenEdges = screenGray.Canny(100, 200))
                using (var templateEdges = templateGray.Canny(100, 200))
                using (var result = screenEdges.MatchTemplate(templateEdges, TemplateMatchingType.CcoeffNormed))
                {
                    result.MinMax(out _, out double[] maxVs, out _, out Point[] maxLocs);
                    if (maxVs[0] >= threshold) return new Rectangle(maxLocs[0], template.Size);
                }
            }

            // Default matching logic
            using (Image<Gray, float> result = screen.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
            {
                result.MinMax(out _, out double[] maxValues, out _, out Point[] maxLocations);
                if (maxValues[0] >= threshold) return new Rectangle(maxLocations[0], template.Size);
            }
            return Rectangle.Empty;
        }

        public void Dispose()
        {
            // cleanup all unmanaged memory
            foreach (var img in _grayCache.Values) img.Dispose();
            foreach (var img in _colorCache.Values) img.Dispose();
            _grayCache.Clear();
            _colorCache.Clear();
            _lruTracker.Clear();
            if (_ocrEngine != null) _ocrEngine.Dispose();
        }
    }
}