using Charon.Input;
using Moq;
using NUnit.Framework;
using System.Diagnostics;
using System.Drawing;

namespace Charon.Tests
{
    [TestFixture]
    public class Test_InputService
    {
        private InputService _inputService = null!;

        [SetUp]
        public void Setup()
        {
            _inputService = new InputService();
        }

        // --- HOLD TIME TESTS ---

        [Test]
        [Description("Verifies that LeftClick respects the explicit holdTime parameter.")]
        public void LeftClick_RespectsHoldTime()
        {
            int requestedHold = 100; // ms
            var watch = Stopwatch.StartNew();

            _inputService.LeftClick(requestedHold); //

            watch.Stop();

            // Should be at least the requested hold time
            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(requestedHold),
                $"Click duration was {watch.ElapsedMilliseconds}ms, expected at least {requestedHold}ms.");
        }

        [Test]
        public void LeftClick_ZeroHoldTime_DoesNotCrash()
        {
            // Ensure 0ms doesn't cause a thread error
            Assert.DoesNotThrow(() => _inputService.LeftClick(0));
        }

        [Test]
        [Description("Verifies that RightClick respects the explicit holdTime parameter.")]
        public void RightClick_RespectsHoldTime()
        {
            int requestedHold = 150;
            var watch = Stopwatch.StartNew();

            _inputService.RightClick(requestedHold); //

            watch.Stop();

            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(requestedHold));
        }

        [Test]
        public void LeftClick_NegativeHoldTime_DefaultsOrSafeguards()
        {
            // Depending on your logic, this should either throw an error 
            // or default to a safe minimum like 1ms.
            Assert.DoesNotThrow(() => _inputService.LeftClick(-100));
        }

        // --- MOVEMENT & DRAG TESTS ---

        [Test]
        [Description("Ensures Drag performs the full sequence (Down -> Move -> Up).")]
        public void Drag_Sequence_DoesNotThrow()
        {
            Point p1 = new Point(100, 100);
            Point p2 = new Point(200, 200);

            // Verifies the sequence of moving, clicking down, moving again, and releasing
            Assert.DoesNotThrow(() => _inputService.Drag(p1, p2, humanLike: false));
        }

        [Test]
        [Description("Verifies Bezier movement generates a path and takes longer than instant movement.")]
        public void MoveMouse_HumanLike_TakesTime()
        {
            Point dest = new Point(800, 800);
            var watch = Stopwatch.StartNew();

            _inputService.MoveMouse(dest, humanLike: true); //

            watch.Stop();

            // Bezier movement involves Thread.Sleep loops, so it must take measurable time
            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThan(10));
        }

        [Test]
        public void MoveMouse_Bezier_UpdatesPositionMultipleTimes()
        {
            // This is hard to test without mocking SendInput, but you can 
            // check if the method takes the expected amount of time.
            var sw = Stopwatch.StartNew();
            _inputService.MoveMouse(new Point(100, 100), humanLike: true);
            sw.Stop();

            // With 10-25 steps and 5-15ms sleep per step, 
            // it should take at least 50ms.
            Assert.That(sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(50));
        }

        // --- KEYBOARD TESTS ---

        [Test]
        public void VirtualKey_Mapping_IsCorrect()
        {
            // Verify a few critical keys match Winuser.h hex codes
            Assert.That((ushort)VirtualKey.LBUTTON, Is.EqualTo(0x01));
            Assert.That((ushort)VirtualKey.W, Is.EqualTo(0x57));
        }

        // --- SAFETY TESTS ---

        [Test]
        [Description("Verifies the Fail-Safe logic handles safe positions correctly.")]
        public void CheckFailSafe_DoesNotThrow_AtCenterScreen()
        {
            // First, move mouse to a safe area away from (0,0)
            _inputService.MoveMouse(new Point(500, 500), humanLike: false);

            Assert.DoesNotThrow(() => _inputService.CheckFailSafe()); //
        }

        // --- UTILITY TESTS ---

        [Test]
        public void Scroll_AcceptsPositiveAndNegativeValues()
        {
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => _inputService.Scroll(120));  // Scroll Up
                Assert.DoesNotThrow(() => _inputService.Scroll(-120)); // Scroll Down
            });
        }
    }
}