using NUnit.Framework;
using Charon.Vision;
using System.Drawing;
using System.Reflection;
using System.Collections;

namespace Charon.Tests
{
    [TestFixture]
    public class Test_VisionService
    {
        private VisionService _service = null!;

        [SetUp]
        public void Setup() => _service = new VisionService();

        [TearDown]
        public void Teardown() => _service.Dispose();

        [Test]
        [Description("Verifies physical pixel reporting. Failure here means the bot will misalign clicks due to Windows Scaling.")]
        public void ScreenResolution_IsDpiAware()
        {
            var res = _service.ScreenResolution;
            Assert.Multiple(() =>
            {
                Assert.That(res.Width, Is.GreaterThanOrEqualTo(800), "DPI awareness failed: Resolution too small.");
                Assert.That(res.Height, Is.GreaterThan(0));
            });
        }

        [Test]
        [Description("Verifies the paint-lock prevents 'Object in use' crashes by synchronizing the conversion process.")]
        public void Capture_Concurrency_IsStable()
        {
            var region = new Rectangle(0, 0, 100, 100);

            Assert.DoesNotThrow(() => {
                // Now stable even with higher thread counts
                Parallel.For(0, 30, i => {
                    using (var img = _service.CaptureRegion(region, useCache: true))
                    {
                        Assert.That(img, Is.Not.Null);
                    }
                });
            });
        }

        [Test]
        [Description("Verifies the cache logic. Reusing buffers is critical for preventing GC stuttering in bots.")]
        public void Capture_Cache_ReusesInternalBitmaps()
        {
            var region = new Rectangle(0, 0, 100, 100);

            // Capture once to fill cache
            _service.CaptureRegion(region, useCache: true).Dispose();

            var field = typeof(VisionService).GetField("_bufferCache", BindingFlags.NonPublic | BindingFlags.Instance);
            var cache = field?.GetValue(_service) as IDictionary;

            Assert.That(cache!.Count, Is.EqualTo(1), "Buffer should be stored in cache.");

            // Capture again with same size
            _service.CaptureRegion(region, useCache: true).Dispose();
            Assert.That(cache.Count, Is.EqualTo(1), "Cache should still have only 1 item (reuse).");
        }

        [Test]
        [Description("Ensures complete memory cleanup. Bots that don't dispose buffers will crash the system within hours.")]
        public void Dispose_PurgesAllCachedBitmaps()
        {
            _service.CaptureRegion(new Rectangle(0, 0, 50, 50), useCache: true);
            _service.Dispose();

            var field = typeof(VisionService).GetField("_bufferCache", BindingFlags.NonPublic | BindingFlags.Instance);
            var cache = field?.GetValue(_service) as IDictionary;

            Assert.That(cache!.Count, Is.EqualTo(0), "Cache was not cleared on Dispose.");
        }
    }
}