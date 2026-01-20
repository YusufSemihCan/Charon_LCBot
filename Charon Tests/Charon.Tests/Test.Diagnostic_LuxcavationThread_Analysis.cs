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
    public class Test_Diagnostic_LuxcavationThread_Analysis
    {
        private VisionLocator _locator = null!;
        private string _assetsPath = @"c:\Users\cany7\source\repos\Charon [LC Bot]\Assets";

        [SetUp]
        public void Setup()
        {
            _locator = new VisionLocator();
            _locator.IndexTemplates(Path.Combine(_assetsPath, "Navigation", "Buttons"));
            _locator.IndexTemplates(Path.Combine(_assetsPath, "Navigation", "Text"));
        }

        [Test]
        public void Diagnose_Thread_Entry_Screen()
        {
            // Analyze the Main Thread Screen
            string ssPath = Path.Combine(_assetsPath, "Navigation", "Screen Resolution", "SR_LuxcavationThread.png");
            if (!File.Exists(ssPath)) { Assert.Ignore("SR_LuxcavationThread.png missing"); return; }

            using var screen = new Image<Bgr, byte>(ssPath);
            Console.WriteLine($"--- Analyzing {Path.GetFileName(ssPath)} ---");

            // Look for Enter Buttons
            var enters = _locator.FindAll(screen, NavigationAssets.ButtonLuxcavationThreadEnter, 0.85);
            Console.WriteLine($"Found {enters.Count} 'Button_LuxcavationThread_Enter'.");

            if (enters.Count > 0)
            {
                enters.Sort((a, b) => a.X.CompareTo(b.X));
                for(int i=0; i<enters.Count; i++)
                {
                    Console.WriteLine($"Enter Button [{i}] (X={enters[i].X}): {enters[i]} {(i==0 ? "<- LEFTMOST" : "")}");
                }
            }
        }

        [Test]
        public void Diagnose_Thread_LevelSelect_Screen()
        {
            // Analyze the Popup Screen
            string ssPath = Path.Combine(_assetsPath, "Navigation", "Screen Resolution", "SR_LuxcavationThread_LevelSelect_1.png");
            if (!File.Exists(ssPath)) { Assert.Ignore("SR_LuxcavationThread_LevelSelect_1.png missing"); return; }

            using var screen = new Image<Bgr, byte>(ssPath);
            Console.WriteLine($"--- Analyzing {Path.GetFileName(ssPath)} ---");

            // Look for Levels
            string[] levels = {
                NavigationAssets.TextLuxcavationThreadLevel60,
                NavigationAssets.TextLuxcavationThreadLevel50,
                NavigationAssets.TextLuxcavationThreadLevel40,
                NavigationAssets.TextLuxcavationThreadLevel30,
                NavigationAssets.TextLuxcavationThreadLevel20
            };

            Rectangle bestLevel = Rectangle.Empty;
            string bestLevelName = "";

            foreach (var lvl in levels)
            {
                var rect = _locator.Find(screen, lvl, 0.85);
                if (!rect.IsEmpty)
                {
                    Console.WriteLine($"[FOUND] {lvl} at {rect}");
                    if (bestLevel.IsEmpty) 
                    {
                        bestLevel = rect;
                        bestLevelName = lvl;
                        Console.WriteLine("-> Highest Level (First Match)");
                    }
                }
            }

            if (!bestLevel.IsEmpty)
            {
                // Look for Enter Buttons
                var levelEnters = _locator.FindAll(screen, NavigationAssets.ButtonLuxcavationThreadLevelEnter, 0.85);
                Console.WriteLine($"Found {levelEnters.Count} 'Button_LuxcavationThread_Level_Enter'.");

                Rectangle matchedBtn = Rectangle.Empty;
                double minDist = double.MaxValue;
                int textMidY = bestLevel.Y + bestLevel.Height / 2;

                foreach (var btn in levelEnters)
                {
                    int btnMidY = btn.Y + btn.Height / 2;
                    if (Math.Abs(btnMidY - textMidY) < 50)
                    {
                         Console.WriteLine($"-> Row Match Candidate at {btn}");
                         double d = Math.Abs(btn.X - bestLevel.X);
                         if (d < minDist)
                         {
                             minDist = d;
                             matchedBtn = btn;
                         }
                    }
                }

                if (!matchedBtn.IsEmpty)
                    Console.WriteLine($"[SUCCESS] Matched Button {matchedBtn} to Text {bestLevelName}");
                else
                    Console.WriteLine("[FAILURE] No row-matched button found.");
            }
        }
    }
}
