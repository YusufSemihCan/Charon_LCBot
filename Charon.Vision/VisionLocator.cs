using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Tesseract;

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

        public void IndexTemplates(string subFolderPath)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subFolderPath);
            if (!Directory.Exists(fullPath)) return;

            // Scan the folder for PNGs
            foreach (var file in Directory.GetFiles(fullPath, "*.png"))
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
        public Rectangle Find(Image<Gray, byte> screen, string templateName, double threshold = 0.9)
        {
            if (!_filePaths.ContainsKey(templateName)) return Rectangle.Empty;

            // A. CHECK RAM (Fastest)
            if (_grayCache.ContainsKey(templateName))
            {
                UpdateLru(templateName); // Mark as "Recently Used"
                return PerformMatch(screen, _grayCache[templateName], threshold);
            }

            // B. LOAD FROM DISK (Slower)
            string path = _filePaths[templateName];
            Image<Gray, byte> loadedImg = new Image<Gray, byte>(path);

            // C. CACHE LOGIC
            if (_mode == CacheMode.Memory)
            {
                // Memory Mode: Use once, then destroy immediately.
                using (loadedImg) return PerformMatch(screen, loadedImg, threshold);
            }
            else
            {
                // Balanced/Speed Mode: Save to RAM (handling eviction if full)
                AddToCache(templateName, loadedImg);
                return PerformMatch(screen, loadedImg, threshold);
            }
        }

        // [COLOR SEARCH] - Precise (Red vs Blue), Higher RAM
        public Rectangle Find(Image<Bgr, byte> screen, string templateName, double threshold = 0.9)
        {
            if (!_filePaths.ContainsKey(templateName)) return Rectangle.Empty;

            // A. CHECK RAM
            if (_colorCache.ContainsKey(templateName))
            {
                UpdateLru(templateName);
                return PerformMatch(screen, _colorCache[templateName], threshold);
            }

            // B. LOAD FROM DISK
            string path = _filePaths[templateName];
            Image<Bgr, byte> loadedImg = new Image<Bgr, byte>(path);

            // C. CACHE LOGIC
            if (_mode == CacheMode.Memory)
            {
                using (loadedImg) return PerformMatch(screen, loadedImg, threshold);
            }
            else
            {
                AddToCache(templateName, loadedImg);
                return PerformMatch(screen, loadedImg, threshold);
            }
        }

        // =========================================================
        //  3. READ FUNCTIONS (OCR / TEXT)
        // =========================================================

        // [STRICT READ] - Expects Gray (Performance safe)
        public string Read(Image<Gray, byte> screen, Rectangle area)
        {
            if (_ocrEngine == null) return "";

            // "ROI" (Region of Interest) lets us look at a specific box without cropping/copying memory.
            screen.ROI = area;
            try
            {
                // Tesseract requires a .NET Bitmap stream.
                using (var bmp = screen.ToBitmap())
                using (var stream = new MemoryStream())
                {
                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

                    using (var pix = Pix.LoadFromMemory(stream.ToArray()))
                    using (var page = _ocrEngine.Process(pix))
                    {
                        return page.GetText().Trim();
                    }
                }
            }
            finally
            {
                screen.ROI = Rectangle.Empty; // CRITICAL: Always reset ROI or future scans will fail.
            }
        }

        // [CONVENIENCE READ] - Accepts Color, converts to Gray automatically.
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
        private Rectangle PerformMatch<TColor, TDepth>(Image<TColor, TDepth> screen, Image<TColor, TDepth> template, double threshold)
            where TColor : struct, IColor
            where TDepth : new()
        {
            // Sanity Check: Needle can't be bigger than the haystack
            if (template.Width > screen.Width || template.Height > screen.Height) return Rectangle.Empty;

            // 'CcoeffNormed' is robust against brightness changes (Flash/Day-Night)
            using (Image<Gray, float> result = screen.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
            {
                result.MinMax(out _, out double[] maxValues, out _, out Point[] maxLocations);

                // If the match score (0.0 to 1.0) is good enough, return the location
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