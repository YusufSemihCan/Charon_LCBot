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

### 3. State Management (`Navigation.cs`)
The bot uses a **Graph-based State Machine** to determine how to move between menus. It identifies its current state using visual anchors (e.g., Active Buttons, Headers).

#### State Graph
- **Window (Dash)**: The central hub.
  - -> Drive
  - -> Sinners
  - -> Charge
- **Sinners**: Character management.
  - -> Window / Drive / Charge
- **Drive**: Mission selection hub.
  - -> **Luxcavation**: Exp / Thread farming (Toggleable).
  - -> **MirrorDungeon**: Dungeons (Handles entry popup).
  - -> Window / Sinners / Charge
- **Charge**: Action point replenishment (Overlay).
  - -> Sub-tabs: Boxes / Modules / Lunacy.
  - -> Exit: Via Cancel button.

### 4. Asset Management
All visual assets keys are stored in `NavigationAssets.cs`. This separates the "What to look for" (Assets) from "How to click it" (Logic).

**New (v1.1): Recursive Loading**
`VisionLocator` now recursively scans the `Assets` folder (e.g., `Assets/Navigation/Text`), allowing for organized folder structures without code changes.

**Luxcavation Logic**
Luxcavation logic now supports distinct **Active** and **Inactive** button states for robust tab detection, and intelligent level selection.
-   **State Detection**: Uses `Button_Active_...` to confirm the current tab.
-   **Transition**: Clicks `Button_InActive_...` to switch tabs.
-   **Level Selection (v1.1)**:
    -   Scans levels 9 down to 1 (`EnterLuxcavationLevel`).
    -   Uses `FindAll` to detect all "Enter" buttons.
    -   Selects the button strictly **row-aligned** with the target level text.
-   **Battle Entry**:
    -   Detects `ToBattle` transition state.
    -   Presses `Enter` key (VirtualKey) to start combat.
-   **Back Navigation**: Prioritizes `ESC` key, falls back to generic `ButtonBack`.

**Charge Menu Logic**
Charge state detection relies on **Active** (Yellow) buttons to identify the current tab.
- **Boxes/Modules/Lunacy**: Detected via `Button_Active_Charge_...`.
- **Navigation**: Clicks `Button_InActive_Charge_...` to switch tabs.
- **Exit**: Uses `Button_Charge_Cancel` to return to the main menu (Window/Drive) when navigating away.

## Usage Example

```csharp
// Simple State Transition
_navigation.NavigateTo(NavigationState.Luxcavation_EXP);

// This automatically:
// 1. Checks current state (e.g., Window)
// 2. Clicks 'Drive' button
// 3. Clicks 'Luxcavation' generic button (if not in Lux)
// 4. Toggles to 'EXP' tab if needed (using Active/Inactive logic)
```
