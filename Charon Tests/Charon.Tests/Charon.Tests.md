# Charon.Tests

## Overview
**Charon.Tests** is the quality assurance project for the solution, implementing NUnit tests to verify core functionalities. It focuses heavily on the `Charon.Vision` library to ensure reliability in image recognition and text reading.

## Project Structure

### Test Classes

#### `Test.VisionLocator.cs`
Primary test suite for verification of the `VisionLocator` class.
-   **Dependencies**: Uses `TestAssets` (images created at runtime or loaded from disk).
-   **Logic**:
    -   **Setup**: Creates dynamic test assets (e.g., specific colored squares, patterns) in a `TestAssets` directory before tests run.
    -   **Teardown**: Disposes locator resources and cleans up temporary files to prevent disk clutter.

## Key Test Cases

### 1. Template Indexing
-   **`IndexTemplates_LoadsMultipleFormats`**: Verifies that the locator correctly scans and indexes different image formats (`.jpg`, `.bmp`) from the assets folder.

### 2. Pattern Matching
-   **`Find_LocatesImage_ExactMatch`**: Tests the basic pattern matching using a "Pixel Perfect" approach on a simulated screen.
-   **`Find_LocatesImage_WithEdges`**: Verifies that the Edge Detection (`Canny`) algorithm correctly identifies shapes even when using a different matching pipeline.

### 3. OCR (Text Reading)
-   **`Read_WithBinarization_ReturnsText`**: Checks if Tesseract can correctly read text ("CHARON") from a generated image.
    -   *Constraint*: Requires `tessdata` to be present in the output directory. Checks for this and ignores the test if missing to prevent false failures.

### 4. Memory Management (Caching)
-   **`VisionLocator_LruEviction_CapsMemory`**: Verifies the "Balanced" cache mode. It loads more items than the `maxCacheSize` allows and asserts that the internal dictionary size does not exceed the limit (verifying Least-Recently-Used eviction).
-   **`Dispose_ClearsInternalCaches`**: Ensures that calling `Dispose()` properly releases all `UnmanagedMemory` (via Emgu.CV `Mat` objects) and clears invalid pointers.

## Running Tests
Run via Visual Studio Test Explorer or CLI:
```powershell
dotnet test "Charon Tests/Charon.Tests/Charon.Tests.csproj"
```
The project includes a `tessdata` folder copy configuration to ensure OCR dependencies are available in the build output.
