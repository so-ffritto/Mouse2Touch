# Mouse2Touch

**Mouse2Touch** is a Windows utility that converts mouse input into touch/pointer events, enabling touch-like interaction on devices that support touch injection — such as touchscreen displays, drawing tablets, or any HID-compatible touch surface — using a regular mouse.

---

## Features

### Trigger Modes
Choose which mouse input triggers touch simulation:

| Mode | Behavior |
|---|---|
| **Not enabled** | Touch simulation is disabled |
| **Mouse side button** | Holding the side button (XButton1) simulates a touch contact; moving the mouse swipes |
| **Right mouse button** | Right-clicking starts a touch swipe; releasing without movement opens the context menu |
| **Keyboard shortcut** | A configurable key combination toggles left-click into touch mode (click + swipe) |

### Touch Simulation
- **Tap**: pressing the trigger acts as a touch-down event at the current cursor position
- **Swipe / Drag**: moving the mouse while the trigger is held moves the touch contact in real time
- **Release**: releasing the trigger lifts the touch contact
- Auto-stops touch if the cursor reaches the screen edge

### Keyboard Shortcut Toggle
- Set any combination of **Ctrl / Alt / Shift + key** as a toggle shortcut
- When active, left-click behaves as touch (tap + swipe)
- An **on-screen overlay (OSD)** confirms activation and deactivation with a visual indicator
- Clicking inside the Mouse2Touch settings window always works normally, even when the toggle is active

### On-Screen Display (OSD)
A pill-shaped overlay appears at the center of the screen when the shortcut toggle changes state:
- ✓ **Blue** = Touch mode ON
- ✗ **Grey** = Touch mode OFF

### System Tray
- The app minimizes to the system tray (bottom-right taskbar area)
- Double-click the tray icon to restore the window
- Right-click the tray icon to show or close the app

### Config Persistence
Settings are automatically saved to `config.json` in the application directory and restored on next launch:
- Selected mode
- Keyboard shortcut

---

## Requirements

- **Windows 10 or later** (touch injection API requires Windows 8+)
- **.NET Framework 4.8**
- A display or device that supports Windows touch input
- The application **always runs as Administrator** (UAC prompt on startup) — required to inject touch events into elevated processes

---

## Installation

No installer required. Download or build the project and run `Mouse2Touch.exe`. A UAC prompt will appear on first launch — administrator privileges are required for touch injection to work correctly across all applications.

---

## Building from Source

1. Open `Mouse2Touch.sln` in **Visual Studio 2019 or later**
2. Target framework: **.NET Framework 4.8**
3. Build → `Mouse2Touch.exe` is output to `bin/Release/`

---

## How It Works

Mouse2Touch hooks into Windows low-level mouse and keyboard events (`WH_MOUSE_LL`, `WH_KEYBOARD_LL`) using the Win32 API. When the configured trigger is detected, the original mouse event is suppressed and a synthetic touch contact is injected using the Windows **Touch Injection API** (`InitializeTouchInjection` / `InjectTouchInput` from `User32.dll`).

The touch contact follows the mouse cursor position in real time on a background thread (~11 ms polling interval), giving smooth swipe and drag behavior.

---

## Known Limitations

- Some applications running as **system administrator** may not receive injected touch events unless Mouse2Touch is also running as administrator
- The mouse cursor remains visible during touch simulation (this is a system-level limitation of the Touch Injection API)

---

## Credits

- Original project: [hbl917070/Mouse2Touch](https://github.com/hbl917070/Mouse2Touch)
- Fork maintained by: [so-ffritto/Mouse2Touch](https://github.com/so-ffritto/Mouse2Touch)
- Low-level hook library: [Gma.UserActivityMonitor](https://www.codeproject.com/Articles/7294/Processing-Global-Mouse-and-Keyboard-Hooks-in-C)
