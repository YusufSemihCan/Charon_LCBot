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
        private VisionLocator _locator;
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
    }
}
