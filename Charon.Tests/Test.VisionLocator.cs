using NUnit.Framework;
using Charon.Vision;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

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

            using (Bitmap bmp = new Bitmap(20, 20))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Red);
                bmp.Save(Path.Combine(_testAssetsPath, "RedSquare.png"), ImageFormat.Png);
            }

            _locator = new VisionLocator(CacheMode.Balanced, maxCacheSize: 5);
            _locator.IndexTemplates("TestAssets");
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
                g.FillRectangle(Brushes.Red, 50, 50, 20, 20);

                using (Image<Bgr, byte> screen = screenBmp.ToImage<Bgr, byte>())
                {
                    Rectangle result = _locator.Find(screen, "RedSquare");

                    // NUnit 4 Syntax
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
            // Generate 6 items (Limit is 5)
            for (int i = 0; i < 6; i++)
            {
                using (Bitmap bmp = new Bitmap(10, 10))
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
    }
}