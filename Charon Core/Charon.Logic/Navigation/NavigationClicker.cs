using Charon.Input;
using Charon.Vision;
using System.Drawing;
using System;
using System.Threading;

namespace Charon.Logic.Navigation
{
    public class NavigationClicker : INavigationClicker
    {
        private readonly IVisionService _vision;
        private readonly IVisionLocator _locator;
        private readonly IInputService _input;

        public NavigationClicker(IVisionService vision, IVisionLocator locator, IInputService input)
        {
            _vision = vision ?? throw new ArgumentNullException(nameof(vision));
            _locator = locator ?? throw new ArgumentNullException(nameof(locator));
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        /// <inheritdoc />
        public bool ClickTemplate(string templateName, double threshold = 0.9)
        {
            var center = FindCenter(templateName, threshold);
            if (center == Point.Empty) return false;

            _input.MoveMouse(center, humanLike: true);
            _input.LeftClick();
            return true;
        }

        /// <inheritdoc />
        public bool ClickHold(string templateName, int durationMs, double threshold = 0.9)
        {
            var center = FindCenter(templateName, threshold);
            if (center == Point.Empty) return false;

            _input.MoveMouse(center, humanLike: true);
            _input.LeftClick(durationMs);
            return true;
        }

        /// <inheritdoc />
        public bool ClickLocation(Rectangle target)
        {
             if (target.IsEmpty) return false;
             Point center = new Point(target.X + target.Width / 2, target.Y + target.Height / 2);
             _input.MoveMouse(center, humanLike: true);
             _input.LeftClick();
             return true;
        }

        /// <inheritdoc />
        public bool Hover(string templateName, double threshold = 0.9)
        {
            var center = FindCenter(templateName, threshold);
            if (center == Point.Empty) return false;

            _input.MoveMouse(center, humanLike: true);
            return true;
        }

        /// <inheritdoc />
        public bool WaitAndClick(string templateName, int timeoutMs = 2000, double threshold = 0.9)
        {
            int elapsed = 0;
            int interval = 200;

            while (elapsed < timeoutMs)
            {
                if (ClickTemplate(templateName, threshold)) return true;

                Thread.Sleep(interval);
                elapsed += interval;
            }
            return false;
        }

        /// <inheritdoc />
        public void TypeInto(string templateName, string text, double threshold = 0.9)
        {
            if (ClickTemplate(templateName, threshold))
            {
                // Wait briefly for focus
                Thread.Sleep(100);

                foreach (char c in text)
                {
                    // Basic alphanumeric mapping would go here
                    // For now, simple assumption that VirtualKey exists or we use Clipboard in future
                    // This is a placeholder for full keyboard mapping
                    ushort vk = (ushort)GetVirtualKey(c);
                    if (vk != 0) _input.PressKey((VirtualKey)vk);
                }
            }
        }

        /// <inheritdoc />
        public void PressKey(VirtualKey key, int holdTime = 20)
        {
             _input.CheckFailSafe();
             _input.PressKey(key, holdTime);
        }

        /// <inheritdoc />
        public void DismissWithEsc()
        {
            PressKey(VirtualKey.ESCAPE);
        }

        // Helper
        private Point FindCenter(string templateName, double threshold)
        {
            _input.CheckFailSafe();
            using var screen = _vision.CaptureRegionGray(_vision.ScreenResolution);
            var rect = _locator.Find(screen, templateName, threshold);

            if (rect.IsEmpty) return Point.Empty;
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        private int GetVirtualKey(char c)
        {
            // Simple helper for basic UPPERCASE ASCII logic
            // In a real app this needs robust mapping
            char upper = char.ToUpper(c);
            if (upper >= 'A' && upper <= 'Z') return upper;
            if (upper >= '0' && upper <= '9') return upper;
            return 0; // Unknown
        }
    }
}