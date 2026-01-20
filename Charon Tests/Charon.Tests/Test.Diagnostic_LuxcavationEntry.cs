using NUnit.Framework;
using Charon.Vision;
using Charon.Logic.Navigation;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.IO;
using System.Collections.Generic;

namespace Charon.Tests
{
    [TestFixture]
    [Explicit("Diagnostic/Integration tests requiring visual environment")]
    public class Test_Diagnostic_LuxcavationEntry
    {
        private VisionLocator _locator = null!;
        private string _assetsPath = @"c:\Users\cany7\source\repos\Charon [LC Bot]\Assets";

        [SetUp]
        public void Setup()
        {
            _locator = new VisionLocator();
            // Point to the absolute Assets folder so it finds them
            _locator.IndexTemplates(@"../../../../../Assets/Navigation/Buttons");
            _locator.IndexTemplates(@"../../../../../Assets/Navigation/Text");
            _locator.IndexTemplates(@"../../../../../Assets/Navigation/Icons");
            
            // Allow manual absolute path indexing if possible, or relative up from bin/Debug/net8.0
            // Test execution dir: bin\Debug\net8.0-windows
            // Assets is in project root. Up 4 levels? 
            // Better: use absolute path if IndexTemplates supports it.
            // On Windows Path.Combine(base, "C:/...") returns "C:/..."
            _locator.IndexTemplates(_assetsPath + @"\Navigation\Buttons");
            _locator.IndexTemplates(_assetsPath + @"\Navigation\Text");
            _locator.IndexTemplates(_assetsPath + @"\Navigation\Icons");
        }

        [Test]
        public void Diagnose_Level_Entry_Logic()
        {
            // Load real screenshot
            string ssPath = Path.Combine(_assetsPath, "Navigation", "Screen Resolution", "SR_LuxcavationEXP_3.png");
            if (!File.Exists(ssPath)) { Assert.Ignore($"Screenshot SR_LuxcavationEXP_3.png not found at {ssPath}"); return; }

            using var screen = new Image<Bgr, byte>(ssPath);
            Console.WriteLine($"--- Analysing {Path.GetFileName(ssPath)} for Rightmost Enter Button ---");

            // Strategy: Find ALL Enter buttons. Sort by X Descending. Pick the first one.
            var allButtons = new List<Rectangle>();
            
            // Search for both variants
            allButtons.AddRange(_locator.FindAll(screen, NavigationAssets.ButtonLuxcavationEnter3, 0.80));
            allButtons.AddRange(_locator.FindAll(screen, NavigationAssets.ButtonLuxcavationEnter2, 0.80));

            Console.WriteLine($"Found {allButtons.Count} potential Enter buttons.");

            if (allButtons.Count > 0)
            {
                // Sort Descending by X (Rightmost first)
                allButtons.Sort((a, b) => b.X.CompareTo(a.X));
                
                var best = allButtons[0];
                Console.WriteLine($"[SUCCESS] Rightmost Enter Button identified at {best}.");
                
                // Draw debug
                screen.Draw(best, new Bgr(0, 255, 0), 3);
                
                // Also draw others in Blue
                for (int i = 1; i < allButtons.Count; i++)
                {
                    screen.Draw(allButtons[i], new Bgr(255, 0, 0), 1);
                }

                string outPath = Path.Combine(Environment.CurrentDirectory, "Diagnostic_Luxcavation_Rightmost.png");
                screen.Save(outPath);
                Console.WriteLine($"Saved diagnostic image to {outPath}");
            }
            else
            {
                Console.WriteLine("[FAILURE] No Enter buttons found using thresholds.");
                // Debug dump
                string dumpPath = Path.Combine(Environment.CurrentDirectory, "Diagnostic_Luxcavation_Fail.png");
                screen.Save(dumpPath);
            }
        }
    }
}
