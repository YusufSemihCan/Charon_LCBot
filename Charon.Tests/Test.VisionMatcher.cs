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
    public class Test_VisionMatcher
    {
        private VisionMatcher _matcher = null!;
        private string _testLibPath = null!;

        [SetUp]
        public void Setup()
        {
            _testLibPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestLibrary");
            Directory.CreateDirectory(_testLibPath);

            CreateTestImage("RedGem.png", Color.Red);
            CreateTestImage("BlueGem.png", Color.Blue);

            _matcher = new VisionMatcher();
            _matcher.LoadLibrary("TestLibrary");
        }

        private void CreateTestImage(string name, Color color)
        {
            using (Bitmap bmp = new Bitmap(32, 32))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(color);
                bmp.Save(Path.Combine(_testLibPath, name), ImageFormat.Png);
            }
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
            using (Bitmap inputBmp = new Bitmap(32, 32))
            using (Graphics g = Graphics.FromImage(inputBmp))
            {
                g.Clear(Color.Red);
                using (Image<Bgr, byte> input = inputBmp.ToImage<Bgr, byte>())
                {
                    MatchResult result = _matcher.Classify(input);

                    // NUnit 4 Syntax
                    Assert.That(result.IsMatch, Is.True);
                    Assert.That(result.Name, Is.EqualTo("RedGem"));
                    Assert.That(result.Score, Is.GreaterThan(0.95));
                }
            }
        }

        [Test]
        public void Classify_Gray_IdentifiesItem()
        {
            using (Bitmap inputBmp = new Bitmap(32, 32))
            using (Graphics g = Graphics.FromImage(inputBmp))
            {
                g.Clear(Color.Blue);
                using (Image<Gray, byte> input = inputBmp.ToImage<Gray, byte>())
                {
                    MatchResult result = _matcher.Classify(input);

                    Assert.That(result.IsMatch, Is.True);
                    Assert.That(result.Name, Is.EqualTo("BlueGem"));
                }
            }
        }
    }
}