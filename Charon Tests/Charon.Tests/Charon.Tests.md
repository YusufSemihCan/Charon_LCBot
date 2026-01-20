# Charon.Tests

## Overview
**Charon.Tests** is the quality assurance project for the solution. It uses **NUnit** to verify the functionality, reliability, and safety of the core Charon libraries.

## Test Suite Details

### Folder Structure
- **Test Assets/**: Contains mock assets for diagnostic tests.
- **Test Results/**: Destination for test logs, screenshots, (`.trx`, `.txt`), and diagnostic output.

### 1. VisionLocator Tests (`Test.VisionLocator.cs`)
Verifies image recognition, OCR accuracy, and memory management.

| Test Name | Description |
|-----------|-------------|
| `IndexTemplates_LoadsMultipleFormats` | Checks that the locator indexes .jpg, .bmp, and .png files correctly. |
| `Find_LocatesImage_ExactMatch` | Verifies pixel-perfect matching of an image on a simulated screen. |
| `Find_LocatesImage_WithEdges` | Verifies that edge detection (`useEdges=true`) can locate shapes. |
| `Find_ReturnsEmpty_WhenTemplateMissing` | Ensures searching for a non-existent template returns `Rectangle.Empty` safely. |
| `Read_WithBinarization_ReturnsText` | Checks if OCR (Tesseract) correctly reads text from a generated image. |
| `VisionLocator_LruEviction_CapsMemory` | Verifies that the cache evicts old items when `CacheMode.Balanced` limit is reached. |
| `Dispose_ClearsInternalCaches` | Ensures `Dispose()` purges all unmanaged resources and internal dictionaries. |

### 2. InputService Tests (`Test.InputService.cs`)
Verifies input simulation, timing accuracy, and safety mechanisms.

| Test Name | Description |
|-----------|-------------|
| `LeftClick_UsesDefaultHoldTime` | Verifies default click hold duration is at least 20ms. |
| `LeftClick_RespectsHoldTime` | Verifies explicit hold duration is respected. |
| `LeftClick_NegativeHoldTime_DoesNotThrow` | Checks that negative hold times are clamped to 0. |
| `RightClick_UsesDefaultHoldTime` | Verifies default right-click hold duration. |
| `RightClick_RespectsHoldTime` | Verifies explicit right-click hold duration. |
| `RightClick_NegativeHoldTime_DoesNotThrow` | Checks safety of negative inputs for right clicks. |
| `PressKey_DefaultHoldTime_Works` | Verifies default key press duration. |
| `PressKey_RespectsHoldTime` | Verifies explicit key press duration. |
| `PressKey_NegativeHoldTime_DoesNotThrow` | Checks safety of negative inputs for key presses. |
| `VirtualKey_Mapping_IsCorrect` | Validates that `VirtualKey` enum maps to correct Win32 codes. |
| `MoveMouse_HumanLike_TakesTime` | Ensures human-like movement is slower/varied compared to instant movement. |
| `MoveMouse_ExtremeCoordinates_DoesNotThrow` | Verifies stability with large coordinate values. |
| `MoveMouse_ActuallyMovesCursor` | **Functional Test**: Moves mouse and verifies physical cursor position matches target. |
| `Drag_Timing_IsCorrect` | Verifies drag sequence includes necessary sleep delays. |
| `Drag_Execution_DoesNotThrow` | Verifies drag operation completes without errors. |
| `Scroll_LargeValues_DoesNotThrow` | Checks safety of large positive/negative scroll amounts. |
| `Scroll_RapidSequence_DoesNotThrow` | Verifies stability under rapid scroll commands. |
| `Scroll_Execution_DoesNotThrow` | Verifies basic scroll execution. |
| `CheckFailSafe_DoesNotThrow_InSafeZone` |  Verifies fail-safe check passes when mouse is not at (0,0). |

### 3. VisionService Tests (`Test.VisionService.cs`)
Verifies screen capture, DPI handling, and buffer reuse.

| Test Name | Description |
|-----------|-------------|
| `ScreenResolution_IsDpiAware` | Checks that reported screen size aligns with physical pixels (DPI aware). |
| `Capture_Concurrency_IsStable` | Stress tests concurrent captures to ensure thread safety. |
| `Capture_Cache_ReusesInternalBitmaps` | Verifies that `useCache=true` reuses existing `Bitmap` objects vs creating new ones. |
| `Dispose_PurgesAllCachedBitmaps` | Checks that specific cached buffers are disposed when the service is disposed. |

### 4. VisionMatcher Tests (`Test.VisionMatcher.cs`)
Verifies item classification logic.

| Test Name | Description |
|-----------|-------------|
| `Classify_Color_IdentifiesRedGem` | Verifies correct classification of a color image against the library. |
| `Classify_Gray_IdentifiesItem` | Verifies correct classification using grayscale logic. |
| `Classify_ReturnsUnknown_WhenNoMatch` | Verifies failure when no matching item is found. |
| `Dispose_ClearsLibrary` | Ensures logic library clears references upon disposal. |

### 5. Navigation Tests (`Test.Navigation.cs`)
Verifies State Machine and Transitions.

| Test Name | Description |
|-----------|-------------|
| `SynchronizeState_DetectsWindow` | Verifies detection of active window anchor. |
| `SynchronizeState_DetectsDrive` | Verifies detection of active drive anchor. |
| `NavigateTo_WindowToDrive` | Verifies correct button click sequence for Window -> Drive. |
| `NavigateTo_DriveToLuxcavation` | Verifies transitions to Luxcavation and EXP toggle. |
| `NavigateTo_HandleMDPopup` | Verifies popup interaction logic (e.g. DungeonProgress). |
| `NavigateTo_LuxcavationToDrive` | Verifies returning to Drive from Luxcavation via Back button. |
| `NavigateTo_MirrorDungeonToDrive` | Verifies returning to Drive from MD via Back button. |

## Running Tests
Run via Visual Studio Test Explorer or CLI:
```powershell
dotnet test "Charon Tests/Charon.Tests/Charon.Tests.csproj"
```
