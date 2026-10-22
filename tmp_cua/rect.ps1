Add-Type '
using System;
using System.Runtime.InteropServices;
public class R {
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    public struct RECT { public int L; public int T; public int Rt; public int B; }
}
'
$p = Get-Process 'Among Us' -ErrorAction Stop
$r = New-Object R+RECT
[R]::GetWindowRect($p.MainWindowHandle, [ref]$r) | Out-Null
Write-Output ("window rect: L=$($r.L) T=$($r.T) R=$($r.Rt) B=$($r.B)  size=$($r.Rt-$r.L)x$($r.B-$r.T)")
Add-Type -AssemblyName System.Windows.Forms
Write-Output ("screen: " + [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.ToString())
