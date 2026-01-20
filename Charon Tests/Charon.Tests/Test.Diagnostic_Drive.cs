using NUnit.Framework;
using Charon.Vision;
using Charon.Logic.Navigation;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.IO;

namespace Charon.Tests
{
    [TestFixture]
    public class Test_Diagnostic_Drive
    {
        private VisionService _vision;
        private VisionLocator _locator;

        [SetUp]
        public void Setup()
        {
            _vision = new VisionService();
            _locator = new VisionLocator();
        }

        [Test]
        public void Check_Drive_Recognition()
        {
            // Capture Color Screen
            using var screen = _vision.CaptureRegion(_vision.ScreenResolution);
            
            // Save debug image to see what Bot sees
            string debugPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Test Results", "Debug_Screen_Drive.png");
            Directory.CreateDirectory(Path.GetDirectoryName(debugPath));
            screen.Save(debugPath);
            Console.WriteLine($"Captured Screen saved to: {debugPath}");

            // Check Active Drive Button
            double scoreActive = 0;
            var rectActive = _locator.Find(screen, NavigationAssets.ButtonActiveDrive, 0.7, false); // Lower threshold to detect even weak matches
            // Inspect internal match score if possible, or just deduce from success.
            // VisionLocator.Find wraps PerformMatch. PerformMatch usually returns Rect.
            // We can't easily get the raw score from public API unless we modify it or infer.
            // But we can check if it found it at 0.7, 0.8, 0.9.
            
            Console.WriteLine($"Checking {NavigationAssets.ButtonActiveDrive}...");
            if (!rectActive.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonActiveDrive} at {rectActive}");
            else Console.WriteLine($"[FAILURE] Did NOT find {NavigationAssets.ButtonActiveDrive} (Threshold 0.7)");
            
            // Check InActive Drive Button (Just in case)
            Console.WriteLine($"Checking {NavigationAssets.ButtonInActiveDrive}...");
            var rectInActive = _locator.Find(screen, NavigationAssets.ButtonInActiveDrive, 0.7, false);
            if (!rectInActive.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonInActiveDrive} at {rectInActive}");
            else Console.WriteLine($"[FAILURE] Did NOT find {NavigationAssets.ButtonInActiveDrive} (Threshold 0.7)");
            
            // Check Window Button (Reference)
            var rectWindow = _locator.Find(screen, NavigationAssets.ButtonActiveWindow, 0.7, false);
             if (!rectWindow.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonActiveWindow} at {rectWindow}");
            else Console.WriteLine($"[FAILURE] Did NOT find {NavigationAssets.ButtonActiveWindow}");
        }
    }
}
