# Charon.Vision

## Overview
**Charon.Vision** is the computer vision module for the Charon automation platform. It is built on top of **Emgu.CV** (OpenCV for .NET) and **Tesseract 5** (OCR). It provides robust and high-performance methods for:
1.  **Screen Capture**: Capturing screen regions or the entire desktop efficiently.
2.  **Pattern Matching**: Finding template images on the screen using various algorithms.
3.  **OCR (Optical Character Recognition)**: Reading text from the screen.

## Project Structure

### Core Components

#### `VisionLocator`
The primary class for handling image recognition and text reading.
-   **Implements**: `IVisionLocator`, `IDisposable`.
-   **Dependencies**: caching system for images (Gray/Color), Tesseract Engine.
-   **Key Methods**:
    -   `IndexTemplates(string folder)`: Scans and loads template images (`.png`, `.jpg`, `.jpeg`, `.bmp`).
    -   `Find(screen, template, threshold, useEdges)`: Locates a template on the screen.
        -   **Grayscale Search**: Fast, robust, low RAM usage.
        -   **Color Search**: Precise (e.g., distinguishing Red vs Blue), higher RAM usage.
        -   **Edge Detection**: Optional Canny edge detection for structural matching.
    -   `Read(image, area)`: Extracts text from a specified region.
        -   Includes automatic binarization/thresholding for better accuracy.
        -   Supports both Grayscale and Color inputs (converts Color to Gray automatically).

#### `VisionMatcher`
A utility class for classifying images against a known library. Useful for identifying items in an inventory or classifying state icons.
-   **Implements**: `IVisionMatcher`, `IDisposable`.
-   **Key Methods**:
    -   `LoadLibrary(string folder)`: Loads a set of reference images.
    -   `Classify(image, threshold)`: Returns the name and score of the best matching template from the library.
    -   **Dual Database**: Maintains both Color and Grayscale versions of templates to avoid runtime conversion overhead.

#### `VisionService`
Handles low-level screen capture operations using Win32 API (`BitBlt`).
-   **Implements**: `IVisionService`, `IDisposable`.
-   **Key Features**:
    -   **DPI Awareness**: Automatically sets process as DPI-aware to ensure pixel-perfect captures.
    -   **Buffer Caching**: Can reuse `Bitmap` buffers to minimize garbage collection pressure (`useCache: true`).
    -   **Thread Safety**: Implements locking (`_paintLock`) when using cached buffers to prevent GDI+ conflicts.
-   **Key Methods**:
    -   `CaptureScreen()`: Captures the primary display.
    -   `CaptureRegion(rect)`: Captures a specific `Rectangle`.
    -   `CaptureRegionGray(rect)`: Captures directly to Grayscale (less memory bandwidth).

## Configuration

### Cache Modes (`CacheMode`)
-   **Speed**: Loads all indexed images into RAM immediately. Highest performance, highest memory usage.
-   **Memory**: Loads images from disk on demand and disposes them immediately. Lowest memory usage, slowest performance (disk I/O).
-   **Balanced**: Keeps frequently used images in RAM up to a limit (`maxCacheSize`), evicting the least recently used (LRU).

## Usage Examples

### 1. Finding an Image
```csharp
// Initialize with Balanced caching
using var locator = new VisionLocator(CacheMode.Balanced);
locator.IndexTemplates("Assets/Buttons");

// Capture screen
using var screen = visionService.CaptureScreen();

// Find "StartButton" template
var match = locator.Find(screen, "StartButton", threshold: 0.9);
if (!match.IsEmpty)
{
    Console.WriteLine($"Found at {match.X}, {match.Y}");
}
```

### 2. Reading Text (OCR)
```csharp
// Define area to read
var textArea = new Rectangle(100, 100, 200, 50);

// Read text
string text = locator.Read(screen, textArea);
Console.WriteLine($"Detected Text: {text}");
```

### 3. Edge-Based Matching
```csharp
// Enable edge detection for better shape matching in varying lighting
var match = locator.Find(screen, "Icon", threshold: 0.8, useEdges: true);
```

## Dependencies
-   `Emgu.CV` (OpenCV for .NET)
-   `Tesseract` (OCR Engine)
-   `System.Drawing.Common` (GDI+ Graphics)
