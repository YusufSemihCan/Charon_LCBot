using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Charon.Vision
{
    public class VisionService : IVisionService, IDisposable
    {
        private readonly Dictionary<Size, Bitmap> _bufferCache = new Dictionary<Size, Bitmap>();

        // Private variable for the "Safety Lock" (prevents double-disposal)
        private bool _disposed = false;

        // Save the dimensions of the screen.
        // Create a public varibale to share with other classes.
        private readonly Rectangle _screenresolution;
        public Rectangle ScreenResolution => _screenresolution;

        // Ask the OS for Screen Resolution
        [DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);

        public VisionService()
        {
            // Assign ScreenResolution acquired from the OS
            int width = GetSystemMetrics(0);
            int height = GetSystemMetrics(1);

            // Store it for later use
            _screenresolution = new Rectangle(0, 0, width, height);
        }

        // Helper to avoid duplicating the "Painting" logic
        private void PaintToBuffer(Bitmap buffer, Rectangle rect)
        {
            using (Graphics graph = Graphics.FromImage(buffer))
            {
                nint hdcDest = graph.GetHdc();
                nint hdcSrc = GetWindowDC(GetDesktopWindow());

                BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y, SRCCOPY);

                ReleaseDC(GetDesktopWindow(), hdcSrc);
                graph.ReleaseHdc(hdcDest);
            }
        }

        // Capture Bitmaps in a memory-efficient way
        private Bitmap CaptureRawBitmap(Rectangle rect, bool useCache)
        {
            // OPTION A: ONE-TIME BUFFER
            if (!useCache)
            {
                Bitmap tempBmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
                PaintToBuffer(tempBmp, rect);
                return tempBmp; // Return a brand new object
            }

            // OPTION B: CACHED BUFFER
            // Check if we have a buffer of this size already if not create one
            if (!_bufferCache.ContainsKey(rect.Size))
            {
                // Create it once
                Bitmap newBuffer = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
                // Put it on the shelf
                _bufferCache.Add(rect.Size, newBuffer);
            }

            // Call the cached buffer
            Bitmap buffer = _bufferCache[rect.Size];

            // Use the buffer to capture the screen region
            PaintToBuffer(buffer, rect);

            return buffer;
        }

        // Ask the OS for Device Context [Scanner]
        // Prepare a blank canvas and copy the image data into it
        // Cleanup the handles we used
        [DllImport("gdi32.dll")] private static extern bool BitBlt(nint hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, nint hdcSrc, int nXSrc, int nYSrc, int dwRop);
        [DllImport("user32.dll")] private static extern nint GetDesktopWindow();
        [DllImport("user32.dll")] private static extern nint GetWindowDC(nint hWnd);
        [DllImport("user32.dll")] private static extern nint ReleaseDC(nint hWnd, nint hDC);
        private const int SRCCOPY = 0x00CC0020;

        // OPTION 1: Get a Standard Color Image
        public Image<Bgr, byte> CaptureRegion(Rectangle rect, bool useCache = false)
        {
            // We call our private helper to get the raw bitmap
            // CRITICAL FIX: Removed 'using' here. We handle disposal manually below.
            Bitmap bmp = CaptureRawBitmap(rect, useCache);

            try
            {
                // Convert to Emgu Color format
                return bmp.ToImage<Bgr, byte>();
            }
            finally
            {
                // If we created a temporary one-time bitmap, we MUST destroy it now.
                // If it is from the cache, we MUST NOT destroy it.
                if (!useCache)
                {
                    bmp.Dispose();
                }
            }
        }

        // OPTION 2: Get a Grayscale Image
        public Image<Gray, byte> CaptureRegionGray(Rectangle rect, bool useCache = false)
        {
            Bitmap bmp = CaptureRawBitmap(rect, useCache);
            try
            {
                // Convert to Emgu Grayscale format directly
                return bmp.ToImage<Gray, byte>();
            }
            finally
            {
                if (!useCache)
                {
                    bmp.Dispose();
                }
            }
        }
        // CLEANUP (IDisposable Implementation)
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
                foreach (var bmp in _bufferCache.Values)
                {
                    bmp.Dispose();
                }
                _bufferCache.Clear();
            }
            _disposed = true;
        }
    }
}