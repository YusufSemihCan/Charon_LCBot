using System.Windows.Input; // This is required for 'Key'

namespace Charon.Input
{
    public interface IInputService
    {
        void Click(int x, int y);

        // Change 'GameKey' to 'Key' here:
        void PressKey(Key key);
    }
}