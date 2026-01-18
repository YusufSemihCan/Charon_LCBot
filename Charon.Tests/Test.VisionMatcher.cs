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
    public class Test_VisionMatcher
    {
        private VisionMatcher _matcher = null!;
        private string _testLibPath = null!;

        [SetUp]
        public void Setup()
        {
            _testLibPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestLibrary");
            Directory.CreateDirectory(_testLibPath);

            using (Bitmap redBmp = GeneratePattern(Color.Red, "R"))
            {
                redBmp.Save(Path.Combine(_testLibPath, "RedGem.png"), ImageFormat.Png);
            }
            using (Bitmap blueBmp = GeneratePattern(Color.Blue, "B"))
            {
                blueBmp.Save(Path.Combine(_testLibPath, "BlueGem.png"), ImageFormat.Png);
            }

            _matcher = new VisionMatcher();
            _matcher.LoadLibrary("TestLibrary");
        }

        private Bitmap GeneratePattern(Color color, string letter)
        {
            Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(color);
                using (Font font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold))
                {
                    g.DrawString(letter, font, Brushes.White, 5, 5);
                }
            }
            return bmp;
        }

        [TearDown]
        public void Teardown()
        {
            _matcher.Dispose();
            if (Directory.Exists(_testLibPath))
                Directory.Delete(_testLibPath, true);
        }

        [Test]
        public void Classify_Color_IdentifiesRedGem()
        {
            using (Bitmap inputBmp = GeneratePattern(Color.Red, "R"))
            using (Image<Bgr, byte> input = inputBmp.ToImage<Bgr, byte>())
            {
                MatchResult result = _matcher.Classify(input);

                Assert.That(result.IsMatch, Is.True, "Failed to match Red Gem");
                Assert.That(result.Name, Is.EqualTo("RedGem"));
                Assert.That(result.Score, Is.GreaterThan(0.95));
            }
        }

        [Test]
        public void Classify_Gray_IdentifiesItem()
        {
            using (Bitmap inputBmp = GeneratePattern(Color.Blue, "B"))
            using (Image<Gray, byte> input = inputBmp.ToImage<Gray, byte>())
            {
                MatchResult result = _matcher.Classify(input);

                Assert.That(result.IsMatch, Is.True, "Failed to match Blue Gem in Gray mode");
                Assert.That(result.Name, Is.EqualTo("BlueGem"));
            }
        }

        // --- NEW MEMORY TEST ---
        [Test]
        public void Dispose_ClearsLibrary()
        {
            // 1. Arrange (Setup has already loaded the library)

            // 2. Act
            _matcher.Dispose();

            // 3. Assert
            var libField = typeof(VisionMatcher).GetField("_libraryColor", BindingFlags.NonPublic | BindingFlags.Instance);
            var library = libField?.GetValue(_matcher) as IDictionary;

            Assert.That(library!.Count, Is.EqualTo(0), "Item Library should be empty after Dispose");
        }
    }
}