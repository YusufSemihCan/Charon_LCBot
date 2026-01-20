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
            // Arrange: Start at Window
            MockState(NavigationState.Window);
            
            // Setup Clicker to succeed
            _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonInActiveDrive, It.IsAny<double>()))
                        .Returns(true);

            // Act
            bool success = _navigation.NavigateTo(NavigationState.Drive);

            // Assert
            Assert.That(success, Is.True);
            _mockClicker.Verify(c => c.ClickTemplate(NavigationAssets.ButtonInActiveDrive, It.IsAny<double>()), Times.Once);
        }

        [Test]
        [Description("Verifies navigation from Drive to Luxcavation (EXP).")]
        public void NavigateTo_DriveToLuxcavation()
        {
            // Arrange: Start at Drive
            MockState(NavigationState.Drive);

            // Setup: First click Luxcavation button, then toggle EXP
            _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonLuxcavation, It.IsAny<double>())).Returns(true);
            _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonLuxcavationEXP, It.IsAny<double>())).Returns(true);

            // Act
            bool success = _navigation.NavigateTo(NavigationState.Luxcavation_EXP);

            // Assert
            Assert.That(success, Is.True);
            _mockClicker.Verify(c => c.ClickTemplate(NavigationAssets.ButtonLuxcavation, It.IsAny<double>()), Times.Once);
            
            // Note: Since NavigateTo is recursive, testing the exact flow requires complex mocking of state changes between calls.
            // For a unit test, we verify the immediate logic of 'NavigateFromDrive'.
            // Determining the *next* recursive call verifies the chain.
            // However, since we mock state based on 'SynchronizeState', we can simulate the transition by changing the return of mockLocator.
        }

        
        // Helper to simulate state in SynchronizeState
        private void MockState(NavigationState state)
        {
            // Reset all finds first
            _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<bool>()))
                        .Returns(Rectangle.Empty);

            string? anchor = null;
            switch (state)
            {
                case NavigationState.Window: anchor = NavigationAssets.ButtonActiveWindow; break;
                case NavigationState.Drive: anchor = NavigationAssets.ButtonActiveDrive; break;
                case NavigationState.Sinners: anchor = NavigationAssets.ButtonActiveSinners; break;
                case NavigationState.MirrorDungeon_Delving: anchor = NavigationAssets.MDDungeonProgress; break;
            }

            if (anchor != null)
            {
                _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), anchor, It.IsAny<double>(), It.IsAny<bool>()))
                            .Returns(new Rectangle(0, 0, 10, 10));
            }
        }
        
        [Test]
        [Description("Verifies navigation from Luxcavation back to Drive via Back button.")]
        public void NavigateTo_LuxcavationToDrive()
        {
             // Arrange
             // First we need to simulate being in the sub-state.
             // But SynchronizeState relies on identifying anchors. 
             // Luxcavation sub-state is identified by 'ButtonTextLuxcavation' + toggle.
             // For this test, we mock the initial find to return a Luxcavation state.
             
             // Navigation.cs: 
             // else if (!_locator.Find(screen, NavigationAssets.ButtonTextLuxcavation).IsEmpty) 
             //    if (!_locator.Find(screen, NavigationAssets.ButtonLuxcavationEXP).IsEmpty) _currentState = NavigationState.Luxcavation_EXP;
             
             _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonTextLuxcavation, It.IsAny<double>(), It.IsAny<bool>()))
                         .Returns(new Rectangle(0,0,10,10));
             _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonLuxcavationEXP, It.IsAny<double>(), It.IsAny<bool>()))
                         .Returns(new Rectangle(0,0,10,10));
             
             // We need to ensure we don't find other anchors like Window/Drive
             _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonActiveWindow, It.IsAny<double>(), It.IsAny<bool>())).Returns(Rectangle.Empty);
             _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonActiveDrive, It.IsAny<double>(), It.IsAny<bool>())).Returns(Rectangle.Empty);
             
             // Setup Back Button Success
             _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonBack, It.IsAny<double>())).Returns(true);
             
             // Act
             bool success = _navigation.NavigateTo(NavigationState.Drive);
             
             // Assert
             Assert.That(success, Is.True);
             _mockClicker.Verify(c => c.ClickTemplate(NavigationAssets.ButtonBack, It.IsAny<double>()), Times.Once);
        }

        [Test]
        [Description("Verifies navigation from MirrorDungeon back to Drive via Back button.")]
        public void NavigateTo_MirrorDungeonToDrive()
        {
             // Arrange: MD State
             // else if (!_locator.Find(screen, NavigationAssets.ButtonTextMD).IsEmpty) -> MirrorDungeon
             _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonTextMD, It.IsAny<double>(), It.IsAny<bool>()))
                         .Returns(new Rectangle(0,0,10,10));
             
             // Ensure no other anchors
             _mockLocator.Setup(l => l.Find(It.IsAny<Image<Gray, byte>>(), NavigationAssets.ButtonActiveDrive, It.IsAny<double>(), It.IsAny<bool>())).Returns(Rectangle.Empty);
             
             // Setup Back Button Success
             _mockClicker.Setup(c => c.ClickTemplate(NavigationAssets.ButtonBack, It.IsAny<double>())).Returns(true);
             
             // Act
             bool success = _navigation.NavigateTo(NavigationState.Drive);
             
             // Assert
             Assert.That(success, Is.True);
             _mockClicker.Verify(c => c.ClickTemplate(NavigationAssets.ButtonBack, It.IsAny<double>()), Times.Once);
        }
    }
}
