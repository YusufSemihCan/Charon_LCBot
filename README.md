# Charon: Modular Vision Automation Framework

> **⚠️ Educational Research Notice**  
> This project is a Proof-of-Concept (PoC) designed to study **Computer Vision-based UI Navigation** and **Hierarchical State Machines**. It is not intended for malicious use, game exploitation, or violation of any Terms of Service.

Charon is an extensible framework that uses **OpenCV (EmguCV)** for visual state detection and a decoupled logic layer to emulate reliable user interactions. Its primary goal is to demonstrate robust state synchronization in dynamic graphical environments.

## ⚖ Disclaimer & Compliance
This software is provided for **educational purposes only**.
- **No Affiliation**: This project is not endorsed by, directly affiliated with, or sponsored by any game developers or software companies.
- **Terms of Service**: Users are strictly responsible for ensuring their usage complies with the Terms of Service of any target application. The developers assume no liability for account actions resulting from misuse.
- **Safety First**: The framework includes built-in fail-safes (e.g., Cursor Escape Protocol) to prevent unintended behavior.

## 🏗 Architecture
The project follows a clean, service-oriented architecture:
* **Vision Service**: High-performance grayscale template matching for real-time state identification.
* **Logic Kernel**: A State Machine capable of recursive navigation and complex decision trees.
* **Input Abstraction**: Safe emulation of input events with human-like delays/patterns.
