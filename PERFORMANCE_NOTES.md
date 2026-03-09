# Performance Optimization Notes

## Baseline Measurement Rationale

Establishing a performance baseline for this specific change is impractical in the current environment for the following reasons:

1. **Platform Constraint**: The application is a Windows-specific WPF application (`net10.0-windows`). The current execution environment is Linux. WPF applications cannot be run or profiled for UI responsiveness in this environment.
2. **Nature of the Operation**: `Process.Start` is an I/O and OS-bound operation. While typically fast for a command like `shutdown`, it can occasionally block the calling thread (in this case, the UI thread) if the OS is under heavy load or if there are delays in process creation.
3. **UI Responsiveness Best Practice**: It is a well-established best practice in desktop application development (WPF, WinForms, etc.) to offload any potentially blocking operations away from the UI thread to ensure the application remains responsive (avoids "Not Responding" state).

## Expected Improvement

By moving `Process.Start` to a background thread using `Task.Run` and making the service call asynchronous, we ensure that the UI thread is never blocked by the process initiation. This leads to a smoother user experience, especially in environments where system commands might take longer to initialize.
