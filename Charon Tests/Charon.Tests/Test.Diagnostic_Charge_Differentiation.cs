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
    public class Test_Diagnostic_Charge_Differentiation
    {
        private VisionLocator _locator = null!;
        private string _assetsPath = @"c:\Users\cany7\source\repos\Charon [LC Bot]\Assets";

        [SetUp]
        public void Setup()
        {
            _locator = new VisionLocator();
            _locator.IndexTemplates(_assetsPath + @"\Navigation\Buttons");
        }

        [Test]
        public void Diagnose_Charge_FalsePositive()
        {
            // We want to test against SR_Charge_Lunacy.jpg
            // In this state: 
            // - Modules should be INACTIVE.
            // - Lunacy should be ACTIVE.
            // Current code checks for Active Modules FIRST. If it finds it, it reports Charge_Modules (WRONG).

            string lunacyScreenPath = Path.Combine(_assetsPath, "Navigation", "Screen Resolution", "SR_Charge_Lunacy.jpg");
            if (!File.Exists(lunacyScreenPath)) { Assert.Ignore("SR_Charge_Lunacy.jpg not found"); return; }

            using var screenBgr = new Image<Bgr, byte>(lunacyScreenPath);
            using var screenGray = screenBgr.Convert<Gray, byte>();

            Console.WriteLine("--- Testing against SR_Charge_Lunacy.jpg ---");

            // 1. Check with Gray (Current Logic)
            Console.WriteLine("[GRAY] Checking 'Button_A_Charge_Modules' (Should FAIL)");
            var rectModGray = _locator.Find(screenGray, NavigationAssets.ButtonActiveChargeModules, 0.9);
            if (!rectModGray.IsEmpty)
                Console.WriteLine($"[FAILURE] Found Active Modules in Gray! Result: {rectModGray}");
            else
                Console.WriteLine("[SUCCESS] Did not find Active Modules in Gray.");

            Console.WriteLine("[GRAY] Checking 'Button_A_Charge_Lunacy' (Should PASS)");
            var rectLunGray = _locator.Find(screenGray, NavigationAssets.ButtonActiveChargeLunacy, 0.9);
            if (!rectLunGray.IsEmpty)
                Console.WriteLine($"[SUCCESS] Found Active Lunacy in Gray! Result: {rectLunGray}");
            else
                Console.WriteLine("[FAILURE] Did not find Active Lunacy in Gray.");


            // 2. Check with Color (Alternative)
            Console.WriteLine("[COLOR] Checking 'Button_A_Charge_Modules' (Should FAIL)");
            var rectModColor = _locator.Find(screenBgr, NavigationAssets.ButtonActiveChargeModules, 0.9); // Note: Find(Bgr) helper might be implicit or explicit
            // Actually IVisionLocator.Find overload for Bgr?
            // VisionLocator.cs: Find(Image<Bgr, byte> screen, string templateName, double threshold = 0.9, bool useEdges = false)
            
             rectModColor = _locator.Find(screenBgr, NavigationAssets.ButtonActiveChargeModules, 0.9);
            if (!rectModColor.IsEmpty)
                Console.WriteLine($"[FAILURE] Found Active Modules in Color! Result: {rectModColor}");
            else
                Console.WriteLine("[SUCCESS] Did not find Active Modules in Color.");

            Console.WriteLine("[COLOR] Checking 'Button_A_Charge_Lunacy' (Should PASS)");
             var rectLunColor = _locator.Find(screenBgr, NavigationAssets.ButtonActiveChargeLunacy, 0.9);
            if (!rectLunColor.IsEmpty)
                Console.WriteLine($"[SUCCESS] Found Active Lunacy in Color! Result: {rectLunColor}");
            else
                Console.WriteLine("[FAILURE] Did not find Active Lunacy in Color.");
        }
    }
}
