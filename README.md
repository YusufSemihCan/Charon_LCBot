# Charon Computer Vision Framework

Charon is aimed to be an efficent automated system to use computer vision to make logic decisions and act accordingly.

## 🏗 Architecture
The project follows a decoupled service architecture to ensure modularity and safety:
* **Vision Service**: Grayscale template matching for high-efficiency state detection.
* **Input Service**: Emulated mouse and keyboard interactions.
* **Logic Layer**: A hierarchical state machine for environment navigation.

## 🛡 Features
* **Visual Anchors**: Robust state synchronization using unique UI identifiers.
* **Fail-Safe Protocol**: Instant process termination if the cursor hits the global escape coordinate (0,0).
* **Modular Assets**: Externalized template management for rapid environment mapping.

## ⚖ Disclaimer
This project is for educational and research purposes only. It is intended to demonstrate the implementation of Computer Vision in automation tasks. Users are responsible for adhering to the Terms of Service of any environment this framework interacts with.
