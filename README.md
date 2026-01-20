# Charon: Modular Vision Automation Framework

> **⚠️ Educational Research Notice**  
> This project is a Proof-of-Concept (PoC) designed to study **Computer Vision-based UI Navigation** and **Hierarchical State Machines**. It is not intended for malicious use, game exploitation, or violation of any Terms of Service.

Charon is an extensible framework that uses **OpenCV (EmguCV)** for visual state detection and a decoupled logic layer to emulate reliable user interactions. Its primary goal is to demonstrate robust state synchronization in dynamic graphical environments.

## ⚖ Disclaimer & Compliance
This software is provided for **educational purposes only**.
- **No Affiliation**: This project is not endorsed by, directly affiliated with, or sponsored by any game developers or software companies.
- **Terms of Service**: Users are strictly responsible for ensuring their usage complies with the Terms of Service of any target application. The developers assume no liability for account actions resulting from misuse.
- **Safety First**: The framework includes built-in fail-safes (e.g., Cursor Escape Protocol) to prevent unintended behavior.

## 🌟 Features

### Robust Navigation System
Charon employs a sophisticated navigation engine capable of handling complex UI hierarchies:
- **State Synchronization**: Uses active polling of visual anchors to determine the current UI state reliably.
- **Chain Navigation**: Can resolve multi-step paths (e.g., navigating from `Window` -> `Drive` -> `Luxcavation`).
- **Overlay Support**: Correctly handles overlay menus (like Charge or Popups) that partially obscure background elements.

### Supported Domains
- **Main Zones**: Window (Dashboard), Drive (Bus), Sinners (Management).
- **Luxcavation**: 
  - Automated entry and type selection (EXP vs Thread).
  - Uses **Color Matching** to distinguish active tabs (Yellow) from inactive ones.
- **Mirror Dungeon**:
  - Handles entry, resume, and confirmation popups.
- **Charge Menu**:
  - Direct access to specific sub-tabs (Boxes, Modules, Lunacy).
  - Intelligent exit strategies using `ESC` or Back buttons.

### Manual Test UI
The application includes a `MainWindow` dashboard for developers to:
- Manually trigger specific navigation commands.
- Visualize the bot's State Detection in real-time.
- Verify asset recognition and logic flows safely.

## 🏗 Architecture
The project follows a clean, service-oriented architecture:
* **Vision Service**: 
  - Supports **Color (BGR)** and Grayscale template matching.
  - Implements caching for high-performance real-time analysis.
* **Logic Kernel**: 
  - A persistent State Machine (`Navigation.cs`) capable of recursive navigation and error recovery.
  - Prioritizes Overlay detection to prevent false positives from background elements.
* **Input Abstraction**: Safe emulation of input events with human-like delays/patterns.

## 🛠 Usage
1. **Build**: Run `dotnet build`.
2. **Setup**: Ensure the `Assets` folder is populated with reference images. (Automatically copied to output).
3. **Run**: Start the application. Use the UI buttons to test specific navigation actions against the target window.

## 🧪 Testing
The framework includes a robust NUnit test suite simulating various UI states using Mock Objects.
- Run `dotnet test` to verify logic integrity.
