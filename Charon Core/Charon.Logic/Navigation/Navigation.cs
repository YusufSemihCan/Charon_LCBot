using Charon.Input;
using Charon.Vision;
using System.Threading;

namespace Charon.Logic.Navigation
{
    public class Navigation
    {
        private readonly NavigationClicker _clicker;
        private readonly IVisionService _vision;
        private readonly IVisionLocator _locator;
        private readonly IInputService _input;
        private NavigationState _currentState = NavigationState.Unknown;

        public Navigation(NavigationClicker clicker, IVisionService vision, IVisionLocator locator, IInputService input)
        {
            _clicker = clicker;
            _vision = vision;
            _locator = locator;
            _input = input;
        }

        public bool NavigateTo(NavigationState target)
        {
            // Global safety check at the start of any navigation path
            _input.CheckFailSafe();

            if (_currentState == target) return true;

            // Connection and Popup handling
            if (_currentState == NavigationState.Connecting || target == NavigationState.Window)
            {
                if (!HandleConnectionAndPopups()) return false;
            }

            // Pathfinding logic
            return target switch
            {
                NavigationState.Window => true,
                NavigationState.Charge => NavigateToWindowThenClick("Btn_Charge", NavigationState.Charge),
                NavigationState.Drive => NavigateToWindowThenClick("Btn_Drive", NavigationState.Drive),
                NavigationState.Luxcavation_EXP => NavigateToDriveThenClick("Btn_Lux_EXP", NavigationState.Luxcavation_EXP),
                NavigationState.MirrorDungeon => NavigateToDriveThenClick("Btn_Mirror_Dungeon", NavigationState.MirrorDungeon),
                _ => false
            };
        }

        private bool HandleConnectionAndPopups()
        {
            int retries = 5;
            while (retries > 0)
            {
                _input.CheckFailSafe();

                using var screen = _vision.CaptureRegionGray(_vision.ScreenResolution);
                if (!_locator.Find(screen, "Anchor_Window_HUD").IsEmpty)
                {
                    _currentState = NavigationState.Window;
                    return true;
                }

                if (_clicker.ClickTemplate("Btn_Retry_Connection") || _clicker.ClickTemplate("Btn_Enter_Game"))
                {
                    Thread.Sleep(3000);
                }

                _clicker.DismissWithEsc();

                retries--;
                Thread.Sleep(1000);
            }
            return false;
        }

        private bool NavigateToWindowThenClick(string template, NavigationState nextState)
        {
            if (NavigateTo(NavigationState.Window))
            {
                if (_clicker.ClickTemplate(template))
                {
                    _currentState = nextState;
                    return true;
                }
            }
            return false;
        }

        private bool NavigateToDriveThenClick(string template, NavigationState nextState)
        {
            // Now using the NavigationAssets constant instead of a raw string
            if (NavigateTo(NavigationState.Drive))
            {
                if (_clicker.ClickTemplate(template))
                {
                    _currentState = nextState;
                    return true;
                }
            }
            return false;
        }
    }
}