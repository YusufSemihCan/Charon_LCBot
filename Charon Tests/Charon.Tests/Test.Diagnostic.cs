using System;
using System.IO;
using System.Drawing;
using NUnit.Framework;
using Emgu.CV;
using Emgu.CV.Structure;
using Charon.Vision;
using Charon.Logic.Navigation;

namespace Charon.Tests
{
    [TestFixture]
    public class Test_Diagnostic
    {
        private VisionLocator _locator = null!;
        private string _assetsPath = @"c:\Users\cany7\source\repos\Charon [LC Bot]\Assets";

        [SetUp]
        public void Setup()
        {
            _locator = new VisionLocator();
        }

        [Test]
        public void Diagnose_LuxcavationEXP_Detection()
        {
            // Load real screenshot
            // Load real screenshot
            string ssPath = Path.Combine(_assetsPath, "Navigation", "Screen Resolution", "SR_LuxcavationEXP_3.png");
            if (!File.Exists(ssPath)) { Assert.Ignore($"Screenshot SR_LuxcavationEXP_3.png not found at {ssPath}"); return; }

            using var screen = new Image<Gray, byte>(ssPath);

            // Test 1: Active Button Detection (Yellow)
            string btnPath = Path.Combine(_assetsPath, "Navigation", NavigationAssets.ButtonActiveLuxcavationEXP + ".png");
            if (File.Exists(btnPath))
            {
                using var btnTemplate = new Image<Gray, byte>(btnPath);
                using (Image<Gray, float> result = screen.MatchTemplate(btnTemplate, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
                {
                    result.MinMax(out _, out double[] maxValues, out _, out Point[] maxLocations);
                    Console.WriteLine($"Button_Active_LuxcavationEXP Match Score: {maxValues[0]}");
                }
            }
        }

        public void Diagnose_LuxcavationThread_Detection()
        {
            string ssPath = Path.Combine(_assetsPath, "Navigation", "Screen Resolution", "SR_LuxcavationThread.png");
            if (!File.Exists(ssPath)) { Assert.Ignore($"Screenshot SR_LuxcavationThread.png not found at {ssPath}"); return; }

            using var screen = new Image<Gray, byte>(ssPath);

            // Test Check: Active Button
             string btnPath = Path.Combine(_assetsPath, "Navigation", NavigationAssets.ButtonActiveLuxcavationThread + ".png");
             if (File.Exists(btnPath))
             {
                 using var btnTemplate = new Image<Gray, byte>(btnPath);
                 using (Image<Gray, float> result = screen.MatchTemplate(btnTemplate, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
                 {
                     result.MinMax(out _, out double[] maxValues, out _, out Point[] maxLocations);
                     Console.WriteLine($"Button_Active_LuxcavationThread Match Score: {maxValues[0]}");
                 }
             }
        }


        [Test]
        public void Diagnose_Drive_Text()
        {
             // Text assets removed by user. 
             Assert.Ignore("Text assets removed.");
        }

        [Test]
        public void Capture_Screen_Snapshot()
        {
            // Helper for User: Captures the screen EXACTLY as the bot sees it.
            // Use this to crop assets instead of Snipping Tool to avoid DPI/Resolution issues.
            string savePath = Path.Combine(_assetsPath, "Raw_Screen_Capture.png");
            
            using var vision = new VisionService(); 
            using var screen = vision.CaptureRegion(vision.ScreenResolution);
            screen.Save(savePath);
            Console.WriteLine($"Snapshot saved to: {savePath}");
        }
    }
}
