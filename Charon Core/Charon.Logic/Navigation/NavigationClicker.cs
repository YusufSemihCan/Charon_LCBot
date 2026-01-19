using Charon.Input;
using Charon.Vision;
using System.Drawing;
using System;

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

        public bool ClickTemplate(string templateName, double threshold = 0.9)
        {
            _input.CheckFailSafe(); // Always verify safety before physical action

            using var screen = _vision.CaptureRegionGray(_vision.ScreenResolution);
            var rect = _locator.Find(screen, templateName, threshold);

            if (rect.IsEmpty) return false;

            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            _input.MoveMouse(center, humanLike: true); // Move using human emulation
            _input.LeftClick(humanLike: true);

            return true;
        }

        public void DismissWithEsc()
        {
            _input.CheckFailSafe();
            _input.PressKey(VirtualKey.ESCAPE, humanLike: true); // Use Esc to clear random popups
        }
    }
}