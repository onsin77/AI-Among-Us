param([int]$x, [int]$y)
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class M2 {
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")] public static extern uint SendInput(uint n, INPUT[] pInputs, int cbSize);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT { public uint type; public MOUSEINPUT mi; }
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT {
        public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr extra;
    }
}
"@
$p = Get-Process "Among Us" -ErrorAction Stop
[M2]::ShowWindow($p.MainWindowHandle, 9) | Out-Null   # SW_RESTORE
[M2]::SetForegroundWindow($p.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 500
$fg = [M2]::GetForegroundWindow()
if ($fg -ne $p.MainWindowHandle) { Write-Output "WARN: foreground=$fg game=$($p.MainWindowHandle)" }
[M2]::SetCursorPos($x, $y) | Out-Null
Start-Sleep -Milliseconds 120
$down = New-Object M2+INPUT
$down.type = 0; $down.mi.dwFlags = 0x0002  # LEFTDOWN
$up = New-Object M2+INPUT
$up.type = 0; $up.mi.dwFlags = 0x0004      # LEFTUP
[M2]::SendInput(1, @($down), [System.Runtime.InteropServices.Marshal]::SizeOf($down)) | Out-Null
Start-Sleep -Milliseconds 60
[M2]::SendInput(1, @($up), [System.Runtime.InteropServices.Marshal]::SizeOf($up)) | Out-Null
Write-Output "SendInput click at $x,$y (fg=$fg)"
