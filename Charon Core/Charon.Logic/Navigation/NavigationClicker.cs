using Charon.Input;
using Charon.Vision;
using System.Drawing;

namespace Charon.Logic.Navigation
{
    public class NavigationClicker
    {
        private readonly IVisionService _vision;
        private readonly IVisionLocator _locator;
        private readonly IInputService _input; // This is the field

        public NavigationClicker(IVisionService vision, IVisionLocator locator, IInputService input)
        {
            _vision = vision;
            _locator = locator;
            _input = input; // Initializing the field
        }

        public bool ClickTemplate(string templateName, double threshold = 0.9)
        {
            // Use the private field '_input'
            _input.CheckFailSafe();

            using var screen = _vision.CaptureRegionGray(_vision.ScreenResolution);
            var rect = _locator.Find(screen, templateName, threshold); //

            if (rect.IsEmpty) return false;

            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2); //
            _input.MoveMouse(center, humanLike: true); //
            _input.LeftClick(humanLike: true); //

            return true;
        }

        public void DismissWithEsc()
        {
            _input.PressKey(VirtualKey.ESCAPE, humanLike: true); //
        }
    }
}