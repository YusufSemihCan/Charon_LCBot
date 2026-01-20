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
        private VisionService _vision = null!;
        private VisionLocator _locator = null!;

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
            string dir = Path.GetDirectoryName(debugPath) ?? string.Empty;
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            screen.Save(debugPath);
            Console.WriteLine($"Captured Screen saved to: {debugPath}");

            // Check Active Drive Button (Icon)
            // Check Active Drive Button (Icon)
            var rectActive = _locator.Find(screen, NavigationAssets.ButtonActiveDrive, 0.7, false);
            
            Console.WriteLine($"Checking {NavigationAssets.ButtonActiveDrive} (ICON)...");
            if (!rectActive.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonActiveDrive} at {rectActive}");
            else Console.WriteLine($"[FAILURE] Did NOT find {NavigationAssets.ButtonActiveDrive} (Threshold 0.7)");
            
            // Check InActive Drive Button (Just in case)
            Console.WriteLine($"Checking {NavigationAssets.ButtonInActiveDrive}...");
            var rectInActive = _locator.Find(screen, NavigationAssets.ButtonInActiveDrive, 0.7, false);
            if (!rectInActive.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonInActiveDrive} at {rectInActive}");
            else Console.WriteLine($"[FAILURE] Did NOT find {NavigationAssets.ButtonInActiveDrive} (Threshold 0.7)");

            // Check InActive Window Button (Drive -> Window Transition)
            Console.WriteLine($"Checking {NavigationAssets.ButtonInActiveWindow}...");
            var rectInActiveWin = _locator.Find(screen, NavigationAssets.ButtonInActiveWindow, 0.7, false);
            if (!rectInActiveWin.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonInActiveWindow} at {rectInActiveWin}");
            else Console.WriteLine($"[FAILURE] Did NOT find {NavigationAssets.ButtonInActiveWindow} (Threshold 0.7)");
            
            // --- LUXCAVATION CHECKS ---
            Console.WriteLine("--- Checking Luxcavation Assets ---");

            // EXP (Active vs Inactive)
            var rectLuxExpActive = _locator.Find(screen, NavigationAssets.ButtonActiveLuxcavationEXP, 0.8, false); // Higher threshold for differentiation
            if (!rectLuxExpActive.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonActiveLuxcavationEXP} (Active)");
            else Console.WriteLine($"[INFO] Did NOT find {NavigationAssets.ButtonActiveLuxcavationEXP} (Active)");

            var rectLuxExpInactive = _locator.Find(screen, NavigationAssets.ButtonInActiveLuxcavationEXP, 0.8, false);
            if (!rectLuxExpInactive.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonInActiveLuxcavationEXP} (Inactive)");
            else Console.WriteLine($"[INFO] Did NOT find {NavigationAssets.ButtonInActiveLuxcavationEXP} (Inactive)");

            // Thread (Active vs Inactive)
            var rectLuxThreadActive = _locator.Find(screen, NavigationAssets.ButtonActiveLuxcavationThread, 0.8, false);
            if (!rectLuxThreadActive.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonActiveLuxcavationThread} (Active)");
            else Console.WriteLine($"[INFO] Did NOT find {NavigationAssets.ButtonActiveLuxcavationThread} (Active)");

            var rectLuxThreadInactive = _locator.Find(screen, NavigationAssets.ButtonInActiveLuxcavationThread, 0.8, false);
            if (!rectLuxThreadInactive.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonInActiveLuxcavationThread} (Inactive)");
            else Console.WriteLine($"[INFO] Did NOT find {NavigationAssets.ButtonInActiveLuxcavationThread} (Inactive)");


            // Check Window Button (Reference)
            var rectWindow = _locator.Find(screen, NavigationAssets.ButtonActiveWindow, 0.7, false);
             if (!rectWindow.IsEmpty) Console.WriteLine($"[SUCCESS] Found {NavigationAssets.ButtonActiveWindow} at {rectWindow}");
            else Console.WriteLine($"[FAILURE] Did NOT find {NavigationAssets.ButtonActiveWindow}");
        }
    }
}
