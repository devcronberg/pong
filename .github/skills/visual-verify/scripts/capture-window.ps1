#requires -Version 5.1
<#
.SYNOPSIS
    Focus the running MonoGame PONG window, send simulated key presses, and save a screenshot.
.DESCRIPTION
    Works around two platform quirks so screenshots of the game are reliable:
      1. SDL (MonoGame DesktopGL) reads keyboard SCANCODES, so keys are sent with KEYEVENTF_SCANCODE.
      2. Windows blocks background focus theft, so AttachThreadInput + a real mouse click are used.
.PARAMETER Keys
    Comma-separated key names to press before capturing, e.g. -Keys Enter or -Keys Enter,F1.
    Supported: Enter, Space, W, S, Up, Down, F1, Escape.
.PARAMETER Out
    Output PNG path (default: paddle-check.png in the current directory).
.PARAMETER ProcessName
    Name of the game process to target (default: Pong).
.EXAMPLE
    ./capture-window.ps1 -Keys Enter -Out paddle-check.png
#>
param(
    [string[]] $Keys = @(),
    [string]   $Out = 'paddle-check.png',
    [string]   $ProcessName = 'Pong'
)

Add-Type -AssemblyName System.Drawing

Add-Type @"
using System;
using System.Runtime.InteropServices;
public class VvWin {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr pid);
    [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
    [DllImport("user32.dll")] public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
    public struct RECT { public int Left, Top, Right, Bottom; }

    public static void ForceForeground(IntPtr hWnd) {
        uint fgThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
        uint thisThread = GetCurrentThreadId();
        AttachThreadInput(thisThread, fgThread, true);
        ShowWindow(hWnd, 9);          // SW_RESTORE
        BringWindowToTop(hWnd);
        SetForegroundWindow(hWnd);
        AttachThreadInput(thisThread, fgThread, false);
    }

    // Send a key by SCANCODE so SDL/MonoGame registers it. flags: 0x08 = KEYEVENTF_SCANCODE, 0x02 = KEYUP
    public static void PressScan(byte scan, bool extended) {
        uint ext = extended ? (uint)0x0001 : 0;   // KEYEVENTF_EXTENDEDKEY for arrow keys etc.
        keybd_event(0, scan, 0x08 | ext, UIntPtr.Zero);
        System.Threading.Thread.Sleep(120);
        keybd_event(0, scan, 0x08 | 0x02 | ext, UIntPtr.Zero);
    }

    public static void ClickAt(int x, int y) {
        SetCursorPos(x, y);
        System.Threading.Thread.Sleep(80);
        mouse_event(0x0002, 0, 0, 0, UIntPtr.Zero);   // left down
        System.Threading.Thread.Sleep(60);
        mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero);   // left up
    }
}
"@

# Scancode set 1. 'extended' flag needed for the grey arrow keys.
$scan = @{
    'Enter'  = @(0x1C, $false)
    'Space'  = @(0x39, $false)
    'W'      = @(0x11, $false)
    'S'      = @(0x1F, $false)
    'Up'     = @(0x48, $true)
    'Down'   = @(0x50, $true)
    'F1'     = @(0x3B, $false)
    'Escape' = @(0x01, $false)
}

$proc = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue |
        Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
if (-not $proc) { Write-Error "Game window not found (process '$ProcessName'). Is the game running?"; exit 1 }
$h = $proc.MainWindowHandle

[VvWin]::ForceForeground($h)
Start-Sleep -Milliseconds 700

$rect = New-Object VvWin+RECT
[VvWin]::GetWindowRect($h, [ref]$rect) | Out-Null
$cx = [int](($rect.Left + $rect.Right) / 2)
$cy = [int](($rect.Top + $rect.Bottom) / 2)

# A real click gives the SDL window keyboard focus.
[VvWin]::ClickAt($cx, $cy)
Start-Sleep -Milliseconds 500
Write-Output ("Foreground now game: {0}" -f ([VvWin]::GetForegroundWindow() -eq $h))

foreach ($k in $Keys) {
    $key = $k.Trim()
    if (-not $scan.ContainsKey($key)) { Write-Warning "Unknown key '$key' - skipped"; continue }
    $entry = $scan[$key]
    [VvWin]::PressScan([byte]$entry[0], [bool]$entry[1])
    Start-Sleep -Milliseconds 500
}
Start-Sleep -Milliseconds 400

# Re-read the rectangle in case the window moved, then capture it.
[VvWin]::GetWindowRect($h, [ref]$rect) | Out-Null
$w = $rect.Right - $rect.Left
$hgt = $rect.Bottom - $rect.Top

$bmp = New-Object System.Drawing.Bitmap($w, $hgt)
$gfx = [System.Drawing.Graphics]::FromImage($bmp)
$gfx.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bmp.Size)
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$gfx.Dispose(); $bmp.Dispose()
Write-Output "Saved: $Out ($w x $hgt)"
