namespace Charon.Logic.Navigation
{
    public interface INavigationClicker
    {
        bool ClickTemplate(string templateName, double threshold = 0.9);
        void DismissWithEsc();
    }
}