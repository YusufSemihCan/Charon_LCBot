using NUnit.Framework;
using Charon.Vision;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections; // Needed for IDictionary
using System.Reflection;  // Needed for Reflection

namespace Charon.Tests
{
    [TestFixture]
    public class Test_VisionLocator
    {
        private VisionLocator _locator = null!;
        private string _testAssetsPath = null!;

        [SetUp]
        public void Setup()
        {
            _testAssetsPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestAssets");
            Directory.CreateDirectory(_testAssetsPath);

            using (Bitmap bmp = GeneratePattern(Color.Red, "A", 20, 20))
            {
                bmp.Save(Path.Combine(_testAssetsPath, "RedSquare.png"), ImageFormat.Png);
            }

            _locator = new VisionLocator(CacheMode.Balanced, maxCacheSize: 5);
            _locator.IndexTemplates("TestAssets");
        }

        private Bitmap GeneratePattern(Color color, string letter, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(color);
                using (Font font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold))
                {
                    g.DrawString(letter, font, Brushes.White, 2, 2);
                }
            }
            return bmp;
        }

        [TearDown]
        public void Teardown()
        {
            _locator.Dispose();
            if (Directory.Exists(_testAssetsPath))
                Directory.Delete(_testAssetsPath, true);
        }

        [Test]
        public void Find_LocatesImage_ExactMatch()
        {
            using (Bitmap screenBmp = new Bitmap(100, 100))
            using (Graphics g = Graphics.FromImage(screenBmp))
            {
                g.Clear(Color.Black);
                using (Image img = Image.FromFile(Path.Combine(_testAssetsPath, "RedSquare.png")))
                {
                    g.DrawImage(img, 50, 50);
                }

                using (Image<Bgr, byte> screen = screenBmp.ToImage<Bgr, byte>())
                {
                    Rectangle result = _locator.Find(screen, "RedSquare");
                    Assert.That(result.X, Is.EqualTo(50));
                    Assert.That(result.Y, Is.EqualTo(50));
                    Assert.That(result.Width, Is.EqualTo(20));
                }
            }
        }

        [Test]
        public void Find_ReturnsEmpty_WhenObjectMissing()
        {
            using (Image<Bgr, byte> blankScreen = new Image<Bgr, byte>(100, 100))
            {
                Rectangle result = _locator.Find(blankScreen, "RedSquare", threshold: 0.99);
                Assert.That(result, Is.EqualTo(Rectangle.Empty));
            }
        }

        [Test]
        public void LruCache_HandlesEvictionWithoutCrash()
        {
            for (int i = 0; i < 6; i++)
            {
                using (Bitmap bmp = GeneratePattern(Color.Blue, i.ToString(), 10, 10))
                {
                    bmp.Save(Path.Combine(_testAssetsPath, $"Item{i}.png"));
                }
            }
            _locator.IndexTemplates("TestAssets");

            using (Image<Gray, byte> dummyScreen = new Image<Gray, byte>(100, 100))
            {
                for (int i = 0; i < 6; i++)
                {
                    _locator.Find(dummyScreen, $"Item{i}");
                }
            }
            Assert.Pass("LRU Cache cycled through items without crashing.");
        }

        // --- NEW MEMORY TEST ---
        [Test]
        public void Dispose_ClearsInternalCaches()
        {
            // 1. Arrange: Load an item into memory
            using (Bitmap screenBmp = new Bitmap(100, 100))
            using (Image<Bgr, byte> screen = screenBmp.ToImage<Bgr, byte>())
            {
                // Force "RedSquare" into the RAM cache
                _locator.Find(screen, "RedSquare");
            }

            // 2. Act
            _locator.Dispose();

            // 3. Assert
            var grayField = typeof(VisionLocator).GetField("_grayCache", BindingFlags.NonPublic | BindingFlags.Instance);
            var colorField = typeof(VisionLocator).GetField("_colorCache", BindingFlags.NonPublic | BindingFlags.Instance);

            var grayCache = grayField?.GetValue(_locator) as IDictionary;
            var colorCache = colorField?.GetValue(_locator) as IDictionary;

            Assert.That(grayCache!.Count, Is.EqualTo(0), "Gray Cache should be empty");
            Assert.That(colorCache!.Count, Is.EqualTo(0), "Color Cache should be empty");
        }
    }
}