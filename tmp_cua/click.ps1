param([int]$x, [int]$y)
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class M {
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
}
"@
$p = Get-Process "Among Us" -ErrorAction Stop
[M]::SetForegroundWindow($p.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 400
[M]::SetCursorPos($x, $y) | Out-Null
Start-Sleep -Milliseconds 150
[M]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)  # LEFTDOWN
Start-Sleep -Milliseconds 40
[M]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)  # LEFTUP
Write-Output "clicked $x,$y"
