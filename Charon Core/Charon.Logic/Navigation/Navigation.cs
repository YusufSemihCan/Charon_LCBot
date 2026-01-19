using Charon.Input;
using Charon.Vision;
using System.Threading;
using System;

using Charon.Logic.Combat;

namespace Charon.Logic.Navigation
{
    public class Navigation : INavigation
    {
        private readonly INavigationClicker _clicker;
        private readonly ICombatClicker _combat;
        private readonly IVisionService _vision;
        private readonly IVisionLocator _locator;
        private readonly IInputService _input;
        private NavigationState _currentState = NavigationState.Unknown;

        public NavigationState CurrentState => _currentState;

        public Navigation(INavigationClicker clicker, ICombatClicker combat, IVisionService vision, IVisionLocator locator, IInputService input)
        {
            _clicker = clicker ?? throw new ArgumentNullException(nameof(clicker));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _vision = vision ?? throw new ArgumentNullException(nameof(vision));
            _locator = locator ?? throw new ArgumentNullException(nameof(locator));
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        public bool NavigateTo(NavigationState target)
        {
            _input.CheckFailSafe(); // Safety check at navigation start

            if (_currentState == target) return true;

            if (_currentState == NavigationState.Connecting || target == NavigationState.Window)
            {
                if (!HandleConnectionAndPopups()) return false;
            }

            // Map-based pathfinding logic
            return target switch
            {
                NavigationState.Window => true,
                NavigationState.Charge => NavigateToWindowThenClick(NavigationAssets.BtnCharge, NavigationState.Charge),
                NavigationState.Drive => NavigateToWindowThenClick(NavigationAssets.BtnDrive, NavigationState.Drive),
                NavigationState.Luxcavation_EXP => NavigateToDriveThenClick(NavigationAssets.BtnLuxExp, NavigationState.Luxcavation_EXP),
                _ => false
            };
        }

        public NavigationState SynchronizeState()
        {
            using var screen = _vision.CaptureRegionGray(_vision.ScreenResolution);

            if (!_locator.Find(screen, NavigationAssets.AnchorWindowHud).IsEmpty)
                _currentState = NavigationState.Window;
            else if (!_locator.Find(screen, NavigationAssets.BtnEnterGame).IsEmpty)
                _currentState = NavigationState.Connecting;
            else
                _currentState = NavigationState.Unknown;

            return _currentState;
        }

        private bool HandleConnectionAndPopups()
        {
            int retries = 5;
            while (retries > 0)
            {
                _input.CheckFailSafe();

                using var screen = _vision.CaptureRegionGray(_vision.ScreenResolution);
                if (!_locator.Find(screen, NavigationAssets.AnchorWindowHud).IsEmpty) // Verify location using anchor
                {
                    _currentState = NavigationState.Window;
                    return true;
                }

                if (_clicker.ClickTemplate(NavigationAssets.BtnRetryConnection) || 
                    _clicker.ClickTemplate(NavigationAssets.BtnEnterGame))
                {
                    Thread.Sleep(3000);
                }

                _clicker.DismissWithEsc(); // Clear random interference

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