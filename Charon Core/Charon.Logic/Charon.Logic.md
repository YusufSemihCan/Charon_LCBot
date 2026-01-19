# Charon.Logic

## Overview
**Charon.Logic** is the decision-making brain of the Charon bot. It orchestrates high-level behaviors by combining `Charon.Vision` (eyes) and `Charon.Input` (hands).

The logic is split into two primary domains:
1.  **Navigation (`Charon.Logic.Navigation`)**: Handles Menu UI, Popups, and State transitions.
2.  **Combat (`Charon.Logic.Combat`)**: Handles Gameplay mechanics, Dragging, and complex pathing.

## Key Components

### 1. Navigation (Menu / UI)
Managed by the `INavigationClicker` interface.

| Method | Description |
|--------|-------------|
| `ClickTemplate` | Finds a visual template and clicks it. |
| `WaitAndClick` | Retries finding a template for a set duration (default 2s) before failing. Useful for loading screens. |
| `ClickHold` | Clicks and holds the left mouse button for a specific duration (e.g., claiming rewards). |
| `TypeInto` | Clicks a field to focus it, then simulates keyboard input for the text. |
| `Hover` | Moves the mouse over an element without clicking (e.g., to reveal tooltips). |
| `PressKey` | Sends a direct virtual key press (e.g., `Esc`, `Enter`, `I` for Inventory). |

### 2. Combat (Gameplay)
Managed by the `ICombatClicker` interface.

| Method | Description |
|--------|-------------|
| `Drag` | Finds two visual points (Start and End) and performs a human-like drag operation between them. |
| `DragChain` | Follows a complex path defined by a sequence of templates. Useful for drawing patterns or navigating maps. |

### 3. State Management
The `Navigation` class maintains the current state of the bot (e.g., `Connecting`, `Window`, `Drive`). It uses the `VisionLocator` to synchronize its internal state with the actual game screen.

## Usage Example

```csharp
// Bootstrapped automatically in BotBootstrapper
public class MyBotScript
{
    private readonly INavigationClicker _nav;
    private readonly ICombatClicker _combat;

    public void Run()
    {
        // Menu Navigation
        if (_nav.WaitAndClick("Btn_Inventory"))
        {
            _nav.TypeInto("Field_Search", "Potion");
        }

        // Gameplay Interaction
        _combat.Drag("Item_Potion", "Slot_Quickbar1");
        
        // Complex Drawing
        _combat.DragChain(new[] { "Rune_Start", "Rune_Mid", "Rune_End" });
    }
}
```
