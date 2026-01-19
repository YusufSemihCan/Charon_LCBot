namespace Charon.Input
{
    // Standard Windows Virtual-Key Codes (Winuser.h)
    public enum VirtualKey : ushort
    {
        // Mouse Buttons
        LBUTTON = 0x01,
        RBUTTON = 0x02,
        MBUTTON = 0x04,

        // Control Keys
        BACKSPACE = 0x08,
        TAB = 0x09,
        ENTER = 0x0D,
        SHIFT = 0x10,
        CONTROL = 0x11,
        ESCAPE = 0x1B,
        SPACE = 0x20,

        // Common Bot Keys
        W = 0x57,
        A = 0x41,
        S = 0x53,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        P = 0x50,

        // --- NEW KEYS FOR COPY/PASTE ---
        C = 0x43, // Copy
        V = 0x56, // Paste
        X = 0x58, // Cut

        // Function Keys
        F1 = 0x70,
        F5 = 0x74,
        F12 = 0x7B
    }
}