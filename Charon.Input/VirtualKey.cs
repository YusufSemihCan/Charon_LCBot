namespace Charon.Input
{
    public enum VirtualKey : ushort
    {
        // Mouse (Used internally by SendInput, but good to have)
        LBUTTON = 0x01,
        RBUTTON = 0x02,
        MBUTTON = 0x04,

        // Specific Keys
        ENTER = 0x0D,
        ESCAPE = 0x1B,
        P = 0x50,

        // Common Utils (We might need these later, but we can keep it minimal)
        SPACE = 0x20,
        SHIFT = 0x10,
        CONTROL = 0x11
    }
}