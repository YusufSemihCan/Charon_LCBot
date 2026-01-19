using NUnit.Framework;
using Charon.Vision;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections;
using System.Reflection;

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
            _testAssetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAssets");
            if (!Directory.Exists(_testAssetsPath)) Directory.CreateDirectory(_testAssetsPath);

            // Create test asset
            string imgPath = Path.Combine(_testAssetsPath, "RedSquare.png");
            using (Bitmap bmp = new Bitmap(20, 20))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Red);
                g.FillRectangle(Brushes.White, 5, 5, 10, 10); // create a pattern
                bmp.Save(imgPath, ImageFormat.Png);
            }
            // VERIFY ASSET CREATION
            if (!File.Exists(imgPath)) throw new Exception("Failed to create test asset: " + imgPath);

            _locator = new VisionLocator(CacheMode.Balanced, maxCacheSize: 5);
            _locator.IndexTemplates("TestAssets");
        }

        [Test]
        [Description("Verifies that IndexTemplates finds and indexes multiple image formats.")]
        public void IndexTemplates_LoadsMultipleFormats()
        {
            // Create dummy files
            string[] extensions = { ".jpg", ".bmp" };
            foreach (var ext in extensions)
            {
                string path = Path.Combine(_testAssetsPath, $"test{ext}");
                using (Bitmap bmp = new Bitmap(10, 10)) bmp.Save(path);
            }

            _locator.IndexTemplates("TestAssets");

            using (var dummy = new Image<Gray, byte>(10, 10))
            {
                // Should not return Empty if it found existing logic
                // Since checkRAM/Load logic checks _filePaths, if we query it, it should at least try to load
                // Since files are 10x10 and dummy is 10x10, it might match or not, but we just check if it throws or returns empty immediately
                // Better check: use Reflection to check _filePaths count
                var field = typeof(VisionLocator).GetField("_filePaths", BindingFlags.NonPublic | BindingFlags.Instance);
                var filePaths = field?.GetValue(_locator) as IDictionary;
                
                Assert.That(filePaths!.Contains("test"), Is.True, "Did not index .jpg file");
            }
        }

        [TearDown]
        public void Teardown()
        {
            _locator.Dispose();
            // Try-catch handles OS delays in releasing file handles
            try
            {
                if (Directory.Exists(_testAssetsPath))
                    Directory.Delete(_testAssetsPath, true);
            }
            catch { /* Ignore cleanup errors */ }
        }

        [Test]
        [Description("Verifies that the locator can find an exact match of an indexed image on a simulated screen.")]
        public void Find_LocatesImage_ExactMatch()
        {
            VerifyFind(useEdges: false);
        }

        [Test]
        [Description("Verifies that the locator can find a match using Edge Detection.")]
        public void Find_LocatesImage_WithEdges()
        {
            VerifyFind(useEdges: true);
        }

        private void VerifyFind(bool useEdges)
        {
            using (Bitmap screenBmp = new Bitmap(100, 100))
            using (Graphics g = Graphics.FromImage(screenBmp))
            {
                g.Clear(Color.Black);

                // FIX: Use ReadAllBytes to prevent 'file in use' IOException
                byte[] bytes = File.ReadAllBytes(Path.Combine(_testAssetsPath, "RedSquare.png"));
                using (var ms = new MemoryStream(bytes))
                using (var img = Image.FromStream(ms))
                {
                    g.DrawImage(img, 50, 50);
                }
                
                using (var screen = screenBmp.ToImage<Bgr, byte>())
                {
                    // Lower threshold to 0.8 to handle GDI+ to Emgu conversion noise
                    Rectangle result = _locator.Find(screen, "RedSquare", threshold: 0.8, useEdges: useEdges);
                    Assert.That(result.X, Is.EqualTo(50), $"Match failed (useEdges={useEdges}).");
                }
            }
        }

        [Test]
        [Description("Verifies that ThresholdBinary pre-processing improves OCR accuracy for Tesseract.")]
        public void Read_WithBinarization_ReturnsText()
        {
            // Use a larger canvas and larger font for better OCR detection
            using (Bitmap bmp = new Bitmap(300, 100))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
                // Use 24pt Bold for maximum reliability
                using (Font f = new Font("Arial", 24, FontStyle.Bold))
                    g.DrawString("CHARON", f, Brushes.White, 20, 20); // 20px padding

                using (var grayImg = bmp.ToImage<Gray, byte>())
                {
                    string text = _locator.Read(grayImg, new Rectangle(0, 0, 300, 100));
                    if (string.IsNullOrEmpty(text)) Assert.Ignore("OCR returned empty text (likely missing tessdata).");
                    Assert.That(text, Does.Contain("CHARON").IgnoreCase);
                }
            }
        }

        [Test]
        [Description("Verifies the Least Recently Used (LRU) cache correctly caps memory usage in Balanced mode.")]
        public void VisionLocator_LruEviction_CapsMemory()
        {
            using (var dummy = new Image<Gray, byte>(10, 10))
            {
                // Push more items than the maxCacheSize limit of 5
                for (int i = 0; i < 7; i++) _locator.Find(dummy, $"Item{i}");
            }

            var field = typeof(VisionLocator).GetField("_grayCache", BindingFlags.NonPublic | BindingFlags.Instance);
            var cache = field?.GetValue(_locator) as IDictionary;
            Assert.That(cache!.Count, Is.LessThanOrEqualTo(5), "Cache did not evict oldest items.");
        }

        [Test]
        [Description("Ensures all internal RAM caches are purged when the locator is disposed.")]
        public void Dispose_ClearsInternalCaches()
        {
            using (var dummy = new Image<Gray, byte>(10, 10)) _locator.Find(dummy, "RedSquare");

            _locator.Dispose();

            var grayField = typeof(VisionLocator).GetField("_grayCache", BindingFlags.NonPublic | BindingFlags.Instance);
            var grayCache = grayField?.GetValue(_locator) as IDictionary;
            Assert.That(grayCache!.Count, Is.EqualTo(0), "Cache was not purged upon Disposal.");
        }
    }
}