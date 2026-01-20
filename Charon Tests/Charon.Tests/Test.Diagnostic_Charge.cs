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
    [Explicit("Diagnostic/Integration tests requiring visual environment")]
    public class Test_Diagnostic_Charge
    {
        private VisionLocator _locator = null!;
        private string _assetsPath = @"c:\Users\cany7\source\repos\Charon [LC Bot]\Assets";

        [SetUp]
        public void Setup()
        {
            _locator = new VisionLocator();
            // Indexing for safety
            _locator.IndexTemplates(_assetsPath); 
            _locator.IndexTemplates(Path.Combine(_assetsPath, "Navigation", "Icons"));
            _locator.IndexTemplates(Path.Combine(_assetsPath, "Navigation", "Buttons"));
        }

        [Test]
        public void Diagnose_Window_To_Charge()
        {
            // PART 1: Finding EnkephalinBox (Jump to Charge)
            string winPath = Path.Combine(_assetsPath, "Navigation", "Screen Resolution", "SR_Window.jpg");
            if (!File.Exists(winPath)) { Assert.Ignore($"Screenshot SR_Window.jpg not found."); return; }
            
            using var winScreen = new Image<Bgr, byte>(winPath);
            Console.WriteLine("--- Checking Window for EnkephalinBox (Jump to Charge) ---");
            
            CheckAsset(winScreen, NavigationAssets.EnkephalinBox); // Icon_EnkephalinBox
            
            // PART 2: Verify Charge State Detection
            string chargePath = Path.Combine(_assetsPath, "Navigation", "Screen Resolution", "SR_Charge_Modules_1.jpg");
             if (!File.Exists(chargePath)) {
                 chargePath = Path.Combine(_assetsPath, "Navigation", "Screen Resolution", "SR_Charge_Modules_2.png");
             }
             
             if (File.Exists(chargePath))
             {
                 using var chargeScreen = new Image<Bgr, byte>(chargePath);
                 Console.WriteLine($"--- Checking Charge Screen ({Path.GetFileName(chargePath)}) ---");
                 
                 // Logic in SynchronizeState checks for these
                 CheckAsset(chargeScreen, NavigationAssets.ButtonActiveChargeModules);
                 CheckAsset(chargeScreen, "Button_A_Charge_Modules"); // Sanity
                 CheckAsset(chargeScreen, NavigationAssets.ButtonActiveChargeLunacy);
             }
             else
             {
                 Console.WriteLine("[WARN] No SR_Charge_Modules screenshot found.");
             }
        }

        private void CheckAsset(Image<Bgr, byte> screen, string assetName)
        {
            // VisionLocator handles Bgr/Gray internally? 
            // If Test_Diagnostic_Drive uses _locator.Find(screen, ...) and screen is Bgr (from Capture), it works.
            // But if it fails, we fall back to manual check.
            
            var rect = _locator.Find(screen.Convert<Gray, byte>(), assetName, 0.85); 
            // Explicit Convert to Gray to be SAFE since I saw Find(Image<Gray...>) in the snippet.
            // If Find(Image<Bgr...>) exists, this is also fine.
            
            if (!rect.IsEmpty)
            {
               Console.WriteLine($"[SUCCESS] Found {assetName} at {rect}");
            }
            else
            {
                // Debug score
                // Try Icons
                string p = Path.Combine(_assetsPath, "Navigation", "Icons", assetName + ".jpg");
                if (!File.Exists(p)) p = Path.Combine(_assetsPath, "Navigation", "Buttons", assetName + ".jpg");
                if (!File.Exists(p)) p = Path.Combine(_assetsPath, "Navigation", "Buttons", assetName + ".png");
                
                if (File.Exists(p))
                {
                     using var t = new Image<Bgr, byte>(p);
                     using var res = screen.MatchTemplate(t, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
                     res.MinMax(out _, out double[] max, out _, out Point[] loc);
                     Console.WriteLine($"[FAILURE] {assetName} not found. MaxScore: {max[0]:F4} at {loc[0]}");
                }
                else
                {
                    Console.WriteLine($"[FAILURE] {assetName} not found (Asset Missing?).");
                }
            }
        }
    }
}
