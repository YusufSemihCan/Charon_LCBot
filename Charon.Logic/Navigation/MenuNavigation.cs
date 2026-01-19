using Charon.Input;
using Charon.Vision;
using System.Threading;

namespace Charon.Logic.Navigation
{
    public class MenuNavigation
    {
        private readonly MenuClicker _clicker;
        private readonly IVisionService _vision;
        private readonly IVisionLocator _locator;
        private readonly IInputService _input;
        private MenuNavigationState _currentState = MenuNavigationState.Unknown;

        public MenuNavigation(MenuClicker clicker, IVisionService vision, IVisionLocator locator, IInputService input)
        {
            _clicker = clicker;
            _vision = vision;
            _locator = locator;
            _input = input;
        }

        public bool NavigateTo(MenuNavigationState target)
        {
            // Global safety check at the start of any navigation path
            _input.CheckFailSafe();

            if (_currentState == target) return true;

            // Connection and Popup handling
            if (_currentState == MenuNavigationState.Connecting || target == MenuNavigationState.Window)
            {
                if (!HandleConnectionAndPopups()) return false;
            }

            // Pathfinding logic
            return target switch
            {
                MenuNavigationState.Window => true,
                MenuNavigationState.Charge => NavigateToWindowThenClick("Btn_Charge", MenuNavigationState.Charge),
                MenuNavigationState.Drive => NavigateToWindowThenClick("Btn_Drive", MenuNavigationState.Drive),
                MenuNavigationState.Luxcavation_EXP => NavigateToDriveThenClick("Btn_Lux_EXP", MenuNavigationState.Luxcavation_EXP),
                MenuNavigationState.MirrorDungeon => NavigateToDriveThenClick("Btn_Mirror_Dungeon", MenuNavigationState.MirrorDungeon),
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
                    _currentState = MenuNavigationState.Window;
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

        private bool NavigateToWindowThenClick(string template, MenuNavigationState nextState)
        {
            if (NavigateTo(MenuNavigationState.Window))
            {
                if (_clicker.ClickTemplate(template))
                {
                    _currentState = nextState;
                    return true;
                }
            }
            return false;
        }

        private bool NavigateToDriveThenClick(string template, MenuNavigationState nextState)
        {
            if (NavigateTo(MenuNavigationState.Drive))
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