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
            string ssPath = Path.Combine(_assetsPath, "SS_LuxcavationEXP.png");
            if (!File.Exists(ssPath)) { Assert.Ignore($"Screenshot SS_LuxcavationEXP.png not found at {ssPath}"); return; }

            using var screen = new Image<Gray, byte>(ssPath);

            // Test 1: Panel Detection
            string panelPath = Path.Combine(_assetsPath, "Navigation", NavigationAssets.PanelLuxcavationEXP + ".png");
            if (!File.Exists(panelPath)) { Assert.Ignore($"Asset {panelPath} not found."); return; }
            
            using var panelTemplate = new Image<Gray, byte>(panelPath);
            
            // Direct EmguCV Match
            using (Image<Gray, float> result = screen.MatchTemplate(panelTemplate, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                result.MinMax(out _, out double[] maxValues, out _, out Point[] maxLocations);
                double matchScore = maxValues[0];
                Console.WriteLine($"Panel_LuxcavationEXP Match Score: {matchScore}");
                
                // Assert it matches at least 0.85 (VisionLocator default is 0.9)
                // If it fails here, we know the Asset is bad or SS is different scaling/resolution.
                Assert.That(matchScore, Is.GreaterThan(0.85), $"Panel_LuxcavationEXP score too low ({matchScore})");
            }

            // Test 2: Active Button Detection (Yellow)
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

        [Test]
        public void Diagnose_LuxcavationThread_Detection()
        {
            string ssPath = Path.Combine(_assetsPath, "SS_LuxcavationThread.png");
            if (!File.Exists(ssPath)) { Assert.Ignore($"Screenshot SS_LuxcavationThread.png not found at {ssPath}"); return; }

            using var screen = new Image<Gray, byte>(ssPath);

            string panelPath = Path.Combine(_assetsPath, "Navigation", NavigationAssets.PanelLuxcavationThread + ".png");
            if (!File.Exists(panelPath)) { Assert.Ignore($"Asset {panelPath} not found."); return; }
            
            using var panelTemplate = new Image<Gray, byte>(panelPath);
            
            using (Image<Gray, float> result = screen.MatchTemplate(panelTemplate, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                result.MinMax(out _, out double[] maxValues, out _, out Point[] maxLocations);
                double matchScore = maxValues[0];
                Console.WriteLine($"Panel_LuxcavationThread Match Score: {matchScore}");
                
                Assert.That(matchScore, Is.GreaterThan(0.85), $"Panel_LuxcavationThread score too low ({matchScore})");
            }
        }


        [Test]
        public void Diagnose_Drive_Text()
        {
            string ssPath = Path.Combine(_assetsPath, "SS_DriveError.png");
            if (!File.Exists(ssPath)) { Assert.Ignore($"Screenshot found at {ssPath}?"); return; }

            using var screen = new Image<Bgr, byte>(ssPath);

            // Asset found in Navigation/Text/Button_Text_Active_Drive.png
            string textPath = Path.Combine(_assetsPath, "Navigation", "Text", NavigationAssets.ButtonTextDrive + ".png");
             if (!File.Exists(textPath)) { Assert.Fail($"Text asset NOT found at {textPath}"); }

            using var template = new Image<Bgra, byte>(textPath);
            Console.WriteLine($"Text Asset Dims: {template.Width}x{template.Height}, Channels: {template.NumberOfChannels}");
            
            using var mask = template[3]; // Alpha channel
            
            // Fix: Use correct MinMax signature for Image class (Arrays)
            mask.MinMax(out double[] min, out double[] max, out Point[] minLoc, out Point[] maxLoc);
            Console.WriteLine($"Alpha Min: {min[0]}, Max: {max[0]}");
            
            using var tBgr = template.Convert<Bgr, byte>();
            
            // For now, testing Standard Matching only to avoid CS1620 on mask
            Console.WriteLine("Using Standard Matching (CcoeffNormed)...");
            using (Image<Gray, float> result = screen.MatchTemplate(tBgr, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                result.MinMax(out _, out double[] maxVal, out _, out Point[] loc);
                Console.WriteLine($"Text Drive Score: {maxVal[0]} at {loc[0]}");
                Assert.That(maxVal[0], Is.GreaterThan(0.50), "Text match score check (Threshold lowered for opaque test)");
            }
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
