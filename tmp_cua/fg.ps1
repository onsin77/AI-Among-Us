Add-Type '
using System;
using System.Runtime.InteropServices;
using System.Text;
public class W {
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
}
'
$h = [W]::GetForegroundWindow()
$sb = New-Object System.Text.StringBuilder 256
[W]::GetWindowText($h, $sb, 256) | Out-Null
$procId = 0
[W]::GetWindowThreadProcessId($h, [ref]$procId) | Out-Null
$proc = Get-Process -Id $procId -ErrorAction SilentlyContinue
Write-Output ("Foreground: '" + $sb.ToString() + "' pid=" + $procId + " name=" + $proc.ProcessName)
