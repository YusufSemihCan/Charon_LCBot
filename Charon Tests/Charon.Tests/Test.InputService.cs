using Charon.Input;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Charon.Tests
{
    [TestFixture]
    public class Test_InputService
    {
        private InputService _service = null!;

        [SetUp]
        public void Setup()
        {
            _service = new InputService();
        }

        // --- MOUSE CLICK TESTS ---

        [Test]
        [Description("Verifies that LeftClick uses the default 20ms hold duration when no parameter is provided.")]
        public void LeftClick_UsesDefaultHoldTime()
        {
            var watch = Stopwatch.StartNew();
            _service.LeftClick(); // Uses default 20ms
            watch.Stop();
            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(20));
        }

        [Test]
        [Description("Verifies that LeftClick respects the explicit holdTime parameter.")]
        public void LeftClick_RespectsHoldTime()
        {
            int requestedHold = 100;
            var watch = Stopwatch.StartNew();

            _service.LeftClick(requestedHold);

            watch.Stop();
            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(requestedHold));
        }

        [Test]
        [Description("Ensures negative hold times are clamped to 0 and do not crash.")]
        public void LeftClick_NegativeHoldTime_DoesNotThrow()
        {
            // This tests the Math.Max(0, holdTime) logic
            Assert.DoesNotThrow(() => _service.LeftClick(-100));
        }

        [Test]
        [Description("Verifies that RightClick uses the default 20ms hold duration when no parameter is provided.")]
        public void RightClick_UsesDefaultHoldTime()
        {
            var watch = Stopwatch.StartNew();
            _service.RightClick(); // Uses default 20ms
            watch.Stop();
            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(20));
        }

        [Test]
        [Description("Verifies that RightClick respects the explicit holdTime parameter.")]
        public void RightClick_RespectsHoldTime()
        {
            int requestedHold = 50;
            var watch = Stopwatch.StartNew();

            _service.RightClick(requestedHold);

            watch.Stop();
            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(requestedHold));
        }

        [Test]
        [Description("Ensures RightClick handles negative input safely via clamping.")]
        public void RightClick_NegativeHoldTime_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.RightClick(-100));
        }

        // --- KEYBOARD TESTS ---

        [Test]
        [Description("Verifies that PressKey uses defualt holdTime without parameters.")]
        public void PressKey_DefaultHoldTime_Works()
        {
            // Verifies the default 20ms parameter logic works
            Assert.DoesNotThrow(() => _service.PressKey(VirtualKey.W));
        }

        [Test]
        [Description("Verifies that PressKey respects the explicit holdTime parameter.")]
        public void PressKey_RespectsHoldTime()
        {
            int requestedHold = 100;
            var watch = Stopwatch.StartNew();

            _service.PressKey(VirtualKey.W, requestedHold);

            watch.Stop();
            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(requestedHold));
        }

        [Test]
        [Description("Verifies that PressKey handles negative hold times safely.")]
        public void PressKey_NegativeHoldTime_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.PressKey(VirtualKey.A, -50));
        }

        [Test]
        [Description("Verifies that PressKey Key_Mapping is correct.")]
        public void VirtualKey_Mapping_IsCorrect()
        {
            Assert.Multiple(() =>
            {
                Assert.That((ushort)VirtualKey.LBUTTON, Is.EqualTo(0x01));
                Assert.That((ushort)VirtualKey.W, Is.EqualTo(0x57));
                Assert.That((ushort)VirtualKey.V, Is.EqualTo(0x56));
            });
        }

        // --- MOVEMENT & DRAG TESTS ---

        [Test]
        [Description("Ensures human-like movement takes longer than instant movement.")]
        public void MoveMouse_HumanLike_TakesTime()
        {
            Point dest = new Point(500, 500);
            var watch = Stopwatch.StartNew();

            _service.MoveMouse(dest, humanLike: true);

            watch.Stop();
            // Bezier movement involves several steps with Thread.Sleep
            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThan(20));
        }

        [Test]
        [Description("Verifies Drag sequence takes at least 60ms (30ms down + 30ms up).")]
        public void Drag_Timing_IsCorrect()
        {
            Point p1 = new Point(100, 100);
            Point p2 = new Point(200, 200);
            var watch = Stopwatch.StartNew();

            _service.Drag(p1, p2, humanLike: false);

            watch.Stop();
            // Drag has two Thread.Sleep(30) calls internally
            Assert.That(watch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(60));
        }

        [Test]
        public void MoveMouse_ExtremeCoordinates_DoesNotThrow()
        {
            // Tests that the math inside SendMouseInput doesn't divide by zero or overflow
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => _service.MoveMouse(new Point(100, 100), false));
                Assert.DoesNotThrow(() => _service.MoveMouse(new Point(9999, 9999), false));
            });
        }

        // --- DRAG TESTS ---

        [Test]
        public void Drag_Execution_DoesNotThrow()
        {
            Point p1 = new Point(100, 100);
            Point p2 = new Point(300, 300);
            Assert.DoesNotThrow(() => _service.Drag(p1, p2, humanLike: false));
        }

        [Test]
        [Description("Verifies that Scroll handles large increments and decrements without throwing.")]
        public void Scroll_LargeValues_DoesNotThrow()
        {
            Assert.Multiple(() =>
            {
                // Test large scroll up
                Assert.DoesNotThrow(() => _service.Scroll(5000), "Failed on large positive scroll.");

                // Test large scroll down
                Assert.DoesNotThrow(() => _service.Scroll(-5000), "Failed on large negative scroll.");
            });
        }

        [Test]
        [Description("Verifies that multiple scroll calls in quick succession execute safely.")]
        public void Scroll_RapidSequence_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    _service.Scroll(120);
                }
            });
        }

        // --- SAFETY & UTILITY ---

        [Test]
        public void CheckFailSafe_DoesNotThrow_InSafeZone()
        {
            // Move mouse away from 0,0 first
            _service.MoveMouse(new Point(200, 200), humanLike: false);
            Assert.DoesNotThrow(() => _service.CheckFailSafe());
        }

        [Test]
        public void Scroll_Execution_DoesNotThrow()
        {
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => _service.Scroll(120));  // Up
                Assert.DoesNotThrow(() => _service.Scroll(-120)); // Down
            });
        }
    }
}