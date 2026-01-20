# Charon

## Overview
**Charon** is the primary executable and user interface for the automation suite. It orchestrates the `Vision` and `Input` services and provides the user controls for starting/stopping the bot.

## Project Structure

### `App.xaml` / `App.xaml.cs`
-   **Role**: Application Entry Point.
-   **Responsibility**: Defines global resources (if any) and startup logic.

### `MainWindow.xaml` / `MainWindow.xaml.cs`
-   **Role**: Main Control Panel.
-   **Key Controls**:
    -   **Start/Stop Button**: Toggles the automation logic execution.
    -   **Enter Best Level**: (Debug) Triggers the Luxcavation Level Selection and Battle Entry logic for verification.
    -   **Status Log (`TextBox`)**: Displays real-time log messages from the bot logic.
-   **Architecture**:
    -   **Service Integration**: Instantiates `VisionService` and `InputService` (currently direct instantiation, structured for Dependency Injection).
    -   **Threading**: Automation runs on a background task (`Task.Run`) to keep the UI responsive.
    -   **Logging**: Uses `Dispatcher.Invoke` to safely update the UI text box from the background thread.

## Key Logic Flow

1.  **Startup**: Application launches `MainWindow`.
2.  **Initialization**: `MainWindow` constructor initializes components and services.
3.  **Execution**:
    -   User clicks **Start**.
    -   `_isRunning` flag is set to `true`.
    -   Background Task starts the "Main Loop".
    -   Logic Component (excluded from this doc) uses Vision/Input services to perform actions.
4.  **Termination**:
    -   User clicks **Stop**.
    -   `_isRunning` flag set to `false`.
    -   Loop terminates gracefully.

## Dependencies
-   **Charon.Vision**: For screen reading.
-   **Charon.Input**: For mouse/keyboard control.
-   **WPF (PresentationCore/PresentationFramework)**: For the UI.
