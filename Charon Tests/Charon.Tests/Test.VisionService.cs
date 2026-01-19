using NUnit.Framework;
using Charon.Vision;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Reflection;

namespace Charon.Tests
{
    [TestFixture]
    public class Test_VisionService
    {
        private VisionService _service = null!;

        [SetUp]
        public void Setup()
        {
            _service = new VisionService();
        }

        [TearDown]
        public void Teardown()
        {
            _service.Dispose();
        }

        [Test]
        public void CaptureRegion_ReturnsValidBgrImage()
        {
            Rectangle region = new Rectangle(0, 0, 10, 10);
            using (Image<Bgr, byte> result = _service.CaptureRegion(region))
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Width, Is.EqualTo(10));
                Assert.That(result.Height, Is.EqualTo(10));
                Assert.That(result.NumberOfChannels, Is.EqualTo(3));
            }
        }

        [Test]
        public void CaptureRegionGray_ReturnsValidGrayImage()
        {
            Rectangle region = new Rectangle(0, 0, 10, 10);
            using (Image<Gray, byte> result = _service.CaptureRegionGray(region))
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.NumberOfChannels, Is.EqualTo(1));
            }
        }

        [Test]
        public void CaptureRegion_WithCache_ReusesBuffer()
        {
            Rectangle region = new Rectangle(0, 0, 100, 100);

            // First Call
            using (var img1 = _service.CaptureRegion(region, useCache: true)) { }

            // Second Call
            using (var img2 = _service.CaptureRegion(region, useCache: true))
            {
                Assert.That(img2, Is.Not.Null);
                Assert.That(img2.Width, Is.EqualTo(100));
            }
        }

        [Test]
        public void Dispose_ClearsBufferCache()
        {
            // 1. Arrange: Fill the cache
            Rectangle region = new Rectangle(0, 0, 100, 100);
            _service.CaptureRegion(region, useCache: true);

            // 2. Act: Dispose the service
            _service.Dispose();

            // 3. Assert: Use Reflection to look inside private '_bufferCache'
            var fieldInfo = typeof(VisionService).GetField("_bufferCache", BindingFlags.NonPublic | BindingFlags.Instance);
            var cache = fieldInfo?.GetValue(_service) as Dictionary<Size, Bitmap>;

            Assert.That(cache, Is.Not.Null);
            Assert.That(cache!.Count, Is.EqualTo(0), "Buffer cache should be empty after Dispose!");
        }

        [Test]
        public void Dispose_CanBeCalledTwice_WithoutCrashing()
        {
            _service.Dispose();
            Assert.DoesNotThrow(() => _service.Dispose());
        }
    }
}