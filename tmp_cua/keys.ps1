param([string]$seq)
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class K {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
}
"@
$p = Get-Process "Among Us" -ErrorAction Stop
[K]::SetForegroundWindow($p.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 300
$map = @{ Down = 0x28; Up = 0x26; Left = 0x25; Right = 0x27; Enter = 0x0D; Escape = 0x1B; Space = 0x20 }
foreach ($k in $seq.Split("+")) {
    $vk = $map[$k]
    if (-not $vk) { Write-Output "unknown: $k"; continue }
    [K]::keybd_event($vk, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 40
    [K]::keybd_event($vk, 0, 2, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 250
    Write-Output "sent $k"
}
