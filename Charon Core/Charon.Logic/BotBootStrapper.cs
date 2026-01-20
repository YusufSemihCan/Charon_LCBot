using Charon.Input;
using Charon.Logic.Navigation;
using Charon.Logic.Combat;
using Charon.Vision;

public class BotBootstrapper
{
    public INavigation Navigation { get; } // Use the interface type here

    public BotBootstrapper()
    {
        var vision = new VisionService();
        var locator = new VisionLocator(CacheMode.Balanced);
        IInputService input = new InputService();

        // 1. Calculate Resolution Scale
        // Reference Resolution: 1920x1080
        // We scale based on HEIGHT (vertical fit is usually most important for UI)
        double scale = vision.ScreenResolution.Height / 1080.0;
        
        // Safety: If something is weird (0 height), default to 1
        if (scale <= 0) scale = 1.0;
        
        locator.ScaleFactor = scale;

        // Link physical images
        string assetsPath = PathResolver.GetNavigationAssetsPath();
        locator.IndexTemplates(assetsPath);

        var clicker = new NavigationClicker(vision, locator, input);
        var combat = new CombatClicker(vision, locator, input);

        // Birth the navigation service
        Navigation = new Navigation(clicker, combat, vision, locator, input);
    }
}