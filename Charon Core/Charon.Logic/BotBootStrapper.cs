using Charon.Input;
using Charon.Logic.Navigation;
using Charon.Vision;

public class BotBootstrapper
{
    public INavigation Navigation { get; } // Use the interface type here

    public BotBootstrapper()
    {
        var vision = new VisionService();
        var locator = new VisionLocator(CacheMode.Balanced);
        var input = new InputService(vision);

        // Link physical images
        string assetsPath = PathResolver.GetNavigationAssetsPath();
        locator.IndexTemplates(assetsPath);

        var clicker = new NavigationClicker(vision, locator, input);

        // Birth the navigation service
        Navigation = new Navigation(clicker, vision, locator, input);
    }
}