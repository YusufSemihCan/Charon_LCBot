# Charon.Input

## Overview
**Charon.Input** is the input simulation library designed to mimic human behavior. It abstracts Win32 `SendInput` calls into high-level, safe, and "human-like" actions to avoid detection by anti-bot systems.

## Project Structure

### `InputService`
The main entry point for all input operations.
-   **Implements**: `IInputService`.
-   **Dependencies**: `user32.dll` (via P/Invoke in `NativeMethods`).

## Key Features

### 1. Human-Like Mouse Movement
Instead of teleporting the cursor or moving in straight lines, `MoveMouse` calculates a **Bezier Curve** path.
-   **Randomization**: Adds random control points to create arcs and deviations.
-   **Variable Speed**: Adjusts sleep times between movement steps to simulate acceleration/deceleration.
-   **Usage**: `MoveMouse(destination, humanLike: true)`

### 2. Fail-Safe Mechanism
**CRITICAL SAFETY FEATURE**: The bot monitors the mouse cursor position.
-   **Trigger**: If the user moves the mouse to the **Top-Left Corner (0, 0)**.
-   **Action**: Throws `OperationCanceledException` immediately.
-   **Integration**: Checked before *every* major action (Click, Move, KeyPress) and periodically during long movements.

### 3. Input Types
-   **Mouse**:
    -   `LeftClick(holdTime)`: Clicks with a configurable hold duration.
    -   `RightClick(holdTime)`: Right-clicks with hold duration.
    -   `Scroll(amount)`: Scrolls the mouse wheel (Positive = Up, Negative = Down).
    -   `Drag(start, end)`: Performs a drag-and-drop operation.
-   **Keyboard**:
    -   `PressKey(key, holdTime)`: Presses and holds a standard Virtual Key (e.g., `VirtualKey.VK_ENTER`).

### 4. Safety & Stability
-   **Thread Safety**: Uses `SendInput` which is the recommended Windows API for injection.
-   **Crash Prevention**: `Thread.Sleep` calls are sanitized to ensure non-negative values.
-   **DPI Awareness**: Coordinates are normalized to 0-65535 absolute range to work across different screen resolutions and DPI modifications.

## Usage Examples

### Natural Mouse Movement
```csharp
var input = new InputService();

// Move to (500, 500) using a curved, natural path
input.MoveMouse(new Point(500, 500), humanLike: true);
```

### Typing (Simulating Key Presses)
```csharp
// Press 'A' key, holding it for approx 50ms
input.PressKey(VirtualKey.VK_A, holdTime: 50);
```

### Drag and Drop
```csharp
var start = new Point(100, 100);
var end = new Point(400, 400);

// Drag file/icon
input.Drag(start, end, humanLike: true);
```

## Dependencies
-   **Windows OS**: Relies on `user32.dll`.
-   **System.Drawing.Common**: For `Point` structures.
