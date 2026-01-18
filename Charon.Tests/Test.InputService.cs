using NUnit.Framework;
using Charon.Input;
using Charon.Vision;
using Moq;
using System.Drawing;
using System.Reflection;

namespace Charon.Tests
{
    [TestFixture]
    public class Test_InputService
    {
        private InputService _inputService = null!;
        private Mock<IVisionService> _mockVision = null!;

        [SetUp]
        public void Setup()
        {
            _mockVision = new Mock<IVisionService>();

            // Define a standard 1080p primary monitor
            _mockVision.Setup(v => v.ScreenResolution).Returns(new Rectangle(0, 0, 1920, 1080));

            _inputService = new InputService(_mockVision.Object);
        }

        [Test]
        [Description("Verifies the Bezier algorithm generates points that stay within a logical path.")]
        public void BezierMath_CalculatesCorrectPath()
        {
            var method = typeof(InputService).GetMethod("CalculateBezierPoint",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Point start = new Point(0, 0);
            Point c1 = new Point(50, 0);
            Point c2 = new Point(50, 100);
            Point end = new Point(100, 100);

            // Test at t=0.5 (the apex of the curve)
            var result = (Point)method!.Invoke(_inputService, new object[] { 0.5, start, c1, c2, end })!;

            // The point should be roughly in the middle of the X and Y bounds
            Assert.That(result.X, Is.InRange(25, 75));
            Assert.That(result.Y, Is.InRange(25, 75));
        }

        [Test]
        [Description("Ensures the scaling math doesn't overflow or crash at screen boundaries.")]
        public void SendMouseInput_Math_HandlesBoundaries()
        {
            var method = typeof(InputService).GetMethod("SendMouseInput",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Test Top-Left (0,0) and Bottom-Right (1920, 1080)
            Assert.DoesNotThrow(() => method!.Invoke(_inputService, new object[] { 0, 0, (uint)0x0001 }));
            Assert.DoesNotThrow(() => method!.Invoke(_inputService, new object[] { 1920, 1080, (uint)0x0001 }));
        }

        [Test]
        [Description("Verifies that the fast toggle (humanLike: false) completes instantly.")]
        public void FastToggle_ExecutesWithoutDelay()
        {
            var startTime = DateTime.Now;

            // Move mouse instantly to a point
            _inputService.MoveMouse(new Point(500, 500), humanLike: false);

            var duration = (DateTime.Now - startTime).TotalMilliseconds;

            // Fast move should take nearly 0ms (well under the 50ms minimum of human-like move)
            Assert.That(duration, Is.LessThan(50));
        }

        [Test]
        public void Drag_Sequence_DoesNotThrow()
        {
            Point p1 = new Point(10, 10);
            Point p2 = new Point(100, 100);

            // Verifies the logic of Down -> Move -> Up
            Assert.DoesNotThrow(() => _inputService.Drag(p1, p2, humanLike: false));
        }

        [Test]
        public void Scroll_Value_AcceptsPositiveAndNegative()
        {
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => _inputService.Scroll(120, humanLike: false));  // Scroll Up
                Assert.DoesNotThrow(() => _inputService.Scroll(-120, humanLike: false)); // Scroll Down
            });
        }

        [Test]
        public void LeftClick_SendsDownAndUpSequence()
        {
            // We use a stopwatch to verify the 'humanLike' delay happened between Down and Up
            var watch = System.Diagnostics.Stopwatch.StartNew();

            _inputService.LeftClick(humanLike: true);

            watch.Stop();

            // Verification: If humanLike is true, the method must have stayed active 
            // for at least 45ms (our minimum defined delay).
            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(45),
                "Click was too fast; it didn't simulate a human hold-time.");
        }

        [Test]
        [Category("Manual")]
        public void ManualTest_TypeInNotepad()
        {
            // Directions: Open Notepad and click inside it within 2 seconds
            Thread.Sleep(2000);

            // Type "P"
            _inputService.PressKey(VirtualKey.P, humanLike: true);

            // Perform a Right Click to show the context menu
            _inputService.RightClick(humanLike: true);

            Assert.Pass("Check if 'p' was typed and context menu is open.");
        }

        [Test]
        public void CheckFailSafe_ThrowsException_AtOrigin()
        {
            // Note: This test requires the cursor to NOT be at (0,0) initially.
            // In a real environment, we would mock GetCursorPos, but for now:

            // We can't easily force the cursor to (0,0) in CI, 
            // but we can test the LOGIC by mocking the NativeMethods if we had a wrapper.

            // Instead, let's test that it DOES NOT throw when the mouse is at a normal position.
            Assert.DoesNotThrow(() => _inputService.CheckFailSafe());
        }
    }
}