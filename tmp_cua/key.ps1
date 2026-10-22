param([string]$k)
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
$vk = @{ Escape = 0x1B; Enter = 0x0D; Tab = 0x09; Space = 0x20; R = 0x52 }[$k]
if (-not $vk) { Write-Output "unknown key $k"; exit 1 }
[K]::keybd_event($vk, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 40
[K]::keybd_event($vk, 0, 2, [UIntPtr]::Zero)
Write-Output "key $k sent"
