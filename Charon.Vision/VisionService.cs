using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Charon.Vision
{
    public class VisionService : IVisionService
    {
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(nint hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, nint hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll")]
        private static extern nint GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern nint GetWindowDC(nint hWnd);

        [DllImport("user32.dll")]
        private static extern nint ReleaseDC(nint hWnd, nint hDC);

        private const int SRCCOPY = 0x00CC0020;


        /// OPTION 1: Get the Standard Color Image (Bgr)
        public Image<Bgr, byte> CaptureRegion(Rectangle rect)
        {
            // We call our private helper to get the raw bitmap
            using (Bitmap bmp = CaptureRawBitmap(rect))
            {
                // Convert to Emgu Color format
                return bmp.ToImage<Bgr, byte>();
            }
        }
        /// OPTION 2: Get the Grayscale Image (Gray)
        /// Faster for pattern matching, uses less memory.
        public Image<Gray, byte> CaptureRegionGray(Rectangle rect)
        {
            using (Bitmap bmp = CaptureRawBitmap(rect))
            {
                // Convert to Emgu Grayscale format directly
                return bmp.ToImage<Gray, byte>();
            }
        }
        /// Captures the screen to a C# Bitmap.
        /// This is private because we don't want the bot logic dealing with raw Bitmaps.
        private Bitmap CaptureRawBitmap(Rectangle rect)
        {
            // Create a blank Bitmap
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);

            // Prepare Graphics objects
            using (Graphics graph = Graphics.FromImage(bmp))
            {
                nint hdcDest = graph.GetHdc();
                nint hdcSrc = GetWindowDC(GetDesktopWindow());

                // 3. Bit Block Transfer (The Snapshot)
                BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y, SRCCOPY);

                // 4. Cleanup Windows handles
                ReleaseDC(GetDesktopWindow(), hdcSrc);
                graph.ReleaseHdc(hdcDest);
            }

            // Return the raw C# bitmap to be converted by the public methods
            return bmp;
        }
    }
}