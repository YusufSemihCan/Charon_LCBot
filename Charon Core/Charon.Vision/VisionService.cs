using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Charon.Vision
{

    /// Handles Image Capture from Screen.
    /// Has option to use Cached Buffers for improved performance.
    /// Has option to capture Color or Grayscale images.
    public class VisionService : IVisionService, IDisposable
    {
        private readonly ConcurrentDictionary<Size, Bitmap> _bufferCache = new ConcurrentDictionary<Size, Bitmap>();
        private readonly object _paintLock = new object(); // Essential for preventing GDI+ "Object in use" crashes
        private bool _disposed = false;

        [DllImport("user32.dll")] private static extern bool SetProcessDPIAware();
        [DllImport("gdi32.dll")] private static extern bool BitBlt(nint hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, nint hdcSrc, int nXSrc, int nYSrc, int dwRop);
        [DllImport("user32.dll")] private static extern nint GetDesktopWindow();
        [DllImport("user32.dll")] private static extern nint GetWindowDC(nint hWnd);
        [DllImport("user32.dll")] private static extern nint ReleaseDC(nint hWnd, nint hDC);
        private const int SRCCOPY = 0x00CC0020;

        private readonly Rectangle _screenResolution;
        public Rectangle ScreenResolution => _screenResolution;

        [DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);

        public VisionService()
        {
            SetProcessDPIAware(); // Ensure physical pixel accuracy
            int width = GetSystemMetrics(0);
            int height = GetSystemMetrics(1);
            _screenResolution = new Rectangle(0, 0, width, height);
        }

        /// Captures screen data into the provided buffer using Win32 BitBlt.
        private void PaintToBuffer(Bitmap buffer, Rectangle rect)
        {
            // Remove the lock from here since it's now handled by the caller
            using (Graphics graph = Graphics.FromImage(buffer))
            {
                nint hdcDest = graph.GetHdc();
                nint hdcSrc = GetWindowDC(GetDesktopWindow());
                try
                {
                    BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y, SRCCOPY);
                }
                finally
                {
                    ReleaseDC(GetDesktopWindow(), hdcSrc);
                    graph.ReleaseHdc(hdcDest);
                }
            }
        }

        /// <summary>
        /// Captures a specific region of the screen.
        /// </summary>
        /// <param name="rect">The area to capture.</param>
        /// <param name="useCache">If true, re-uses an existing buffer (faster but requires thread safety).</param>
        /// <returns>A Color image of the region.</returns>
        public Image<Bgr, byte> CaptureRegion(Rectangle rect, bool useCache = false)
        {
            // If using cache, we must lock the entire process because Emgu's ToImage 
            // reads Bitmap properties that GDI+ considers 'in use' during a BitBlt.
            if (useCache)
            {
                lock (_paintLock)
                {
                    Bitmap bmp = CaptureRawBitmap(rect, useCache);
                    return bmp.ToImage<Bgr, byte>();
                }
            }

            // Non-cached version uses a unique object, so it's thread-safe by default.
            Bitmap tempBmp = CaptureRawBitmap(rect, useCache);
            try { return tempBmp.ToImage<Bgr, byte>(); }
            finally { tempBmp.Dispose(); }
        }

        /// <summary>
        /// Captures the entire primary screen.
        /// </summary>
        /// <param name="useCache">If true, re-uses buffers.</param>
        /// <returns>A Color image of the screen.</returns>
        public Image<Bgr, byte> CaptureScreen(bool useCache = false)
        {
            return CaptureRegion(_screenResolution, useCache);
        }

        /// <summary>
        /// Captures a specific region of the screen as Grayscale.
        /// </summary>
        /// <param name="rect">The area to capture.</param>
        /// <param name="useCache">If true, re-uses buffers.</param>
        /// <returns>A Grayscale image of the region.</returns>
        public Image<Gray, byte> CaptureRegionGray(Rectangle rect, bool useCache = false)
        {
            if (useCache)
            {
                lock (_paintLock)
                {
                    Bitmap bmp = CaptureRawBitmap(rect, useCache);
                    return bmp.ToImage<Gray, byte>();
                }
            }

            Bitmap tempBmp = CaptureRawBitmap(rect, useCache);
            try { return tempBmp.ToImage<Gray, byte>(); }
            finally { tempBmp.Dispose(); }
        }

        private Bitmap CaptureRawBitmap(Rectangle rect, bool useCache)
        {
            if (!useCache)
            {
                Bitmap tempBmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
                PaintToBuffer(tempBmp, rect);
                return tempBmp;
            }

            Bitmap buffer = _bufferCache.GetOrAdd(rect.Size, s =>
                new Bitmap(s.Width, s.Height, PixelFormat.Format24bppRgb));

            PaintToBuffer(buffer, rect);
            return buffer;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                foreach (var bmp in _bufferCache.Values) bmp.Dispose();
                _bufferCache.Clear();
            }
            _disposed = true;
        }
    }
}