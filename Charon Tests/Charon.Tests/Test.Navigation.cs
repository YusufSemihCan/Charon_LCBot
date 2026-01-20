using NUnit.Framework;
using Moq;
using Charon.Logic.Navigation;
using Charon.Logic.Combat;
using Charon.Vision;
using Charon.Input;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Charon.Tests
{
    [TestFixture]
    public class Test_Navigation
    {
        private Mock<INavigationClicker> _mockClicker = null!;
        private Mock<ICombatClicker> _mockCombat = null!;
        private Mock<IVisionService> _mockVision = null!;
        private Mock<IVisionLocator> _mockLocator = null!;
        private Mock<IInputService> _mockInput = null!;
        
        private Navigation _navigation = null!;

        [SetUp]
        public void Setup()
        {
            _mockClicker = new Mock<INavigationClicker>();
            _mockCombat = new Mock<ICombatClicker>();
            _mockVision = new Mock<IVisionService>();
            _mockLocator = new Mock<IVisionLocator>();
            _mockInput = new Mock<IInputService>();

            // Mock Vision capture to return a dummy image (disposable)
            _mockVision.Setup(v => v.CaptureRegionGray(It.IsAny<Rectangle>(), It.IsAny<bool>()))
                       .Returns(new Image<Gray, byte>(100, 100));

            _navigation = new Navigation(
                _mockClicker.Object,
                _mockCombat.Object,
                _mockVision.Object,
                _mockLocator.Object,
                _mockInput.Object
            );
        }

        [Test]
        [Description("Verifies that SynchronizeState detects Window when the anchor is present.")]
        public void SynchronizeState_DetectsWindow()
        {
            // Arrange: Locator detects Window Button
            _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonActiveWindow, It.IsAny<double>(), It.IsAny<bool>()))
                        .Returns(new Rectangle(0, 0, 10, 10));

            // Act
            var state = _navigation.SynchronizeState();

            // Assert
            Assert.That(state, Is.EqualTo(NavigationState.Window));
        }

        [Test]
        [Description("Verifies that SynchronizeState detects Drive when the anchor is present.")]
        public void SynchronizeState_DetectsDrive()
        {
            // Arrange: Locator detects Drive Button
            _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonActiveDrive, It.IsAny<double>(), It.IsAny<bool>()))
                        .Returns(new Rectangle(0, 0, 10, 10));

            // Act
            var state = _navigation.SynchronizeState();

            // Assert
            Assert.That(state, Is.EqualTo(NavigationState.Drive));
        }

        [Test]
        [Description("Verifies navigation from Window to Drive.")]
        public void NavigateTo_WindowToDrive()
        {
            // Arrange
            SetupTransition(NavigationState.Window, NavigationState.Drive);
            
            _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonInActiveDrive, It.IsAny<double>()))
                        .Returns(true);

            // Act
            bool success = _navigation.NavigateTo(NavigationState.Drive);

            // Assert
            Assert.That(success, Is.True);
        }

        [Test]
        [Description("Verifies navigation from Drive to Luxcavation (EXP).")]
        public void NavigateTo_DriveToLuxcavation()
        {
            // Arrange
            SetupTransition(NavigationState.Drive, NavigationState.Luxcavation_EXP);

            _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonLuxcavation, It.IsAny<double>())).Returns(true);
            _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonLuxcavationEXP, It.IsAny<double>())).Returns(true);

            // Act
            bool success = _navigation.NavigateTo(NavigationState.Luxcavation_EXP);

            // Assert
            Assert.That(success, Is.True);
        }

        [Test]
        [Description("Verifies navigation from Luxcavation back to Drive via Back button.")]
        public void NavigateTo_LuxcavationToDrive()
        {
            // Arrange
            SetupTransition(NavigationState.Luxcavation_EXP, NavigationState.Drive);
             
            _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonBack, It.IsAny<double>())).Returns(true);
             
            // Act
            bool success = _navigation.NavigateTo(NavigationState.Drive);
             
            // Assert
            Assert.That(success, Is.True);
        }

        [Test]
        [Description("Verifies navigation from MirrorDungeon back to Drive via Back button.")]
        public void NavigateTo_MirrorDungeonToDrive()
        {
            // Arrange
            SetupTransition(NavigationState.MirrorDungeon_Delving, NavigationState.Drive);
             
            _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonBack, It.IsAny<double>())).Returns(true);
             
            // Act
            bool success = _navigation.NavigateTo(NavigationState.Drive);
             
            // Assert
            Assert.That(success, Is.True);
        }

        [Test]
        [Description("Verifies entering MirrorDungeon Delving state via Enter button (and resulting popup).")]
        public void NavigateTo_MirrorDungeon_Enter()
        {
            // Arrange
            SetupTransition(NavigationState.MirrorDungeon, NavigationState.MirrorDungeon_Delving);

            _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonMDEnter, It.IsAny<double>())).Returns(true);

            // Act
            bool success = _navigation.NavigateTo(NavigationState.MirrorDungeon_Delving);

            // Assert
            Assert.That(success, Is.True);
            _mockClicker.Verify(c => c.ClickTemplate(NavigationAssets.ButtonMDEnter, It.IsAny<double>()), Times.Once);
        }

        private void SetupTransition(NavigationState from, NavigationState to)
        {
            // Reset mocks
            _mockLocator.Reset();

            // Helper to get anchor for a state
            string? GetAnchor(NavigationState s) => s switch {
                NavigationState.Window => NavigationAssets.ButtonActiveWindow,
                NavigationState.Drive => NavigationAssets.ButtonActiveDrive,
                NavigationState.Sinners => NavigationAssets.ButtonActiveSinners,
                NavigationState.MirrorDungeon => NavigationAssets.ButtonTextMD,
                NavigationState.MirrorDungeon_Delving => NavigationAssets.ButtonTextMD,
                NavigationState.Luxcavation_EXP => NavigationAssets.ButtonTextLuxcavation,
                _ => null
            };

            var fromAnchor = GetAnchor(from);
            var toAnchor = GetAnchor(to);
            var rect = new Rectangle(0, 0, 10, 10);

            // Determine Priority (Order of checks in SynchronizeState)
            // 1. Window, 2. Drive, 3. Sinners, 4. Charge, 5. Luxcavation, 6. MD
            int GetPriority(string? anchor)
            {
                if (anchor == NavigationAssets.ButtonActiveWindow) return 1;
                if (anchor == NavigationAssets.ButtonActiveDrive) return 2;
                if (anchor == NavigationAssets.ButtonActiveSinners) return 3;
                if (anchor == NavigationAssets.ChargeLabel) return 4;
                if (anchor == NavigationAssets.ButtonTextLuxcavation) return 5;
                if (anchor == NavigationAssets.ButtonTextMD) return 6;
                return 100;
            }

            int fromPrio = GetPriority(fromAnchor);
            int toPrio = GetPriority(toAnchor);

            if (fromAnchor != null)
            {
                // FromAnchor is always found 1st time.
                // It is checked 2nd time (End Sync). It must NOT be found.
                _mockLocator.SetupSequence(l => l.Find(It.IsAny<Image<Gray, byte>>(), fromAnchor, It.IsAny<double>(), It.IsAny<bool>()))
                            .Returns(rect)
                            .Returns(Rectangle.Empty);
            }

            if (toAnchor != null && toAnchor != fromAnchor)
            {
                if (toPrio < fromPrio)
                {
                    // "To" is checked BEFORE "From".
                    // Pass 1: "To" checked -> Must be Empty (Not there yet).
                    // Pass 2: "To" checked -> Must be Found.
                    _mockLocator.SetupSequence(l => l.Find(It.IsAny<Image<Gray, byte>>(), toAnchor, It.IsAny<double>(), It.IsAny<bool>()))
                                .Returns(Rectangle.Empty)
                                .Returns(rect);
                }
                else
                {
                    // "To" is checked AFTER "From".
                    // Pass 1: "To" NOT CHECKED (Because From was found).
                    // Pass 2: "To" checked -> Found.
                    // So we only need it to return Rect once (or always).
                    _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), toAnchor, It.IsAny<double>(), It.IsAny<bool>()))
                                .Returns(rect);
                }
            }
            else if (toAnchor != null && toAnchor == fromAnchor)
            {
                 // Recursive/Sub-state transition. Anchor must REMAIN found.
                 // Overwrite the 'fromAnchor' sequence which sets it to Empty.
                 _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), toAnchor, It.IsAny<double>(), It.IsAny<bool>()))
                             .Returns(rect);
            }
            
            // Sub-state logic for Luxcavation
            if (from == NavigationState.Luxcavation_EXP)
            {
                 // Checked AFTER FromAnchor(LuxText). 
                 // Pass 1: Found. Pass 2: Not Found.
                  _mockLocator.SetupSequence(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonLuxcavationEXP, It.IsAny<double>(), It.IsAny<bool>()))
                            .Returns(rect)
                            .Returns(Rectangle.Empty);
            }
            if (to == NavigationState.Luxcavation_EXP)
            {
                 // Checked AFTER ToAnchor(LuxText).
                 // Pass 1: Not reached? Or Not Found.
                 // If From=Drive(2), To=Lux(5).
                 // Pass 1: Drive Found. Lux NOT checked.
                 // Pass 2: Drive NOT Found. Lux Found. LuxEXP Checked -> Found.
                 _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonLuxcavationEXP, It.IsAny<double>(), It.IsAny<bool>()))
                            .Returns(rect);
            }
            
            // Sub-state logic for MirrorDungeon_Delving
            if (from == NavigationState.MirrorDungeon_Delving)
            {
                 // Check MDDungeonProgress
                 _mockLocator.SetupSequence(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.MDDungeonProgress, It.IsAny<double>(), It.IsAny<bool>()))
                            .Returns(rect)
                            .Returns(Rectangle.Empty);
            }
            if (to == NavigationState.MirrorDungeon_Delving)
            {
                 // Needs MDDungeonProgress
                 // Use Sequence: Not Found (Start) -> Found (End) -> Found (Persistence)
                 _mockLocator.SetupSequence(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.MDDungeonProgress, It.IsAny<double>(), It.IsAny<bool>()))
                            .Returns(Rectangle.Empty)
                            .Returns(rect)
                            .Returns(rect)
                            .Returns(rect);
            }
        }
        
        [Test]
        [Description("Verifies entering MirrorDungeon Delving state via Enter button with Confirmation Popup.")]
        public void NavigateTo_MirrorDungeon_Enter_WithConfirmation()
        {
            // Arrange
            // Start at MirrorDungeon (Parent)
            // Mock Transition:
            // 1. Click Enter -> Confrmation Popup appears (State is still MirrorDungeon or just NOT Delving yet)
            // 2. Click Enter again -> Delving Popup appears (State becomes Delving)
            
            // Setup Mocks manually for this complex sequence or enhance SetupTransition?
            // Let's do manual for clarity as SetupTransition is getting complex.
            
            _mockLocator.Reset();
            var rect = new Rectangle(0,0,10,10);
            
            // Sequence of States:
            // 1. Start: MD (Text Found, Progress Empty)
            // 2. Click 1: MD (Text Found, Progress Empty) <- Still MD, not Delving
            // 3. Click 2: Delving (Text Found, Progress Found)
            
            _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonTextMD, It.IsAny<double>(), It.IsAny<bool>()))
                        .Returns(rect); // Always in MD
            
            _mockLocator.SetupSequence(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.MDDungeonProgress, It.IsAny<double>(), It.IsAny<bool>()))
                        .Returns(Rectangle.Empty) // Start
                        .Returns(Rectangle.Empty) // After 1st Click (Confirmation Popup)
                        .Returns(rect);       // After 2nd Click
            
            // Ensure other high-priority anchors are empty
            _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonActiveWindow, It.IsAny<double>(), It.IsAny<bool>())).Returns(Rectangle.Empty);
            _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonActiveDrive, It.IsAny<double>(), It.IsAny<bool>())).Returns(Rectangle.Empty);
            _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonActiveSinners, It.IsAny<double>(), It.IsAny<bool>())).Returns(Rectangle.Empty);
            _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ChargeLabel, It.IsAny<double>(), It.IsAny<bool>())).Returns(Rectangle.Empty);


            _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonMDEnter, It.IsAny<double>())).Returns(true);

            // Act
            bool success = _navigation.NavigateTo(NavigationState.MirrorDungeon_Delving);

            // Assert
            Assert.That(success, Is.True);
            _mockClicker.Verify(c => c.ClickTemplate(NavigationAssets.ButtonMDEnter, It.IsAny<double>()), Times.Exactly(2));
        }
    }
}
