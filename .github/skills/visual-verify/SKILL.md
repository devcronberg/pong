---
name: visual-verify
description: "Visually verify the running MonoGame PONG game by capturing a screenshot of its window. Use when you need to confirm on-screen appearance — paddle/ball colours, layout, screen text, or any rendered state — that unit tests cannot check. Drives the game with simulated key presses (Enter to serve, W/S/Up/Down, F1 debug) and saves a PNG you can inspect."
---

# Visual Verification (screenshot the running game)

Use this when a change affects **what the player sees** (colours, positions, on-screen
text, debug overlays) and you need to confirm it in the actual running window — something
`dotnet test` cannot validate.

The bundled script [scripts/capture-window.ps1](scripts/capture-window.ps1) handles the
tricky parts (window focus + SDL input) so screenshots are reliable.

## Why a script is needed

- MonoGame DesktopGL uses **SDL**, which reads keyboard **scancodes**, not virtual keys —
  ordinary `SendKeys`/`keybd_event` with virtual keys is ignored. The script sends
  `KEYEVENTF_SCANCODE` input.
- Windows blocks background processes from stealing focus. The script uses the
  `AttachThreadInput` workaround plus a real mouse click to give the SDL window input focus.
- Paddles/ball are only drawn **after** leaving the `Welcome` screen, so the game must be
  advanced with `Enter` before the screenshot is useful.

## Step 1 – Start the game

Launch it in the background so the window renders while you keep working:

```powershell
dotnet run --project Pong.csproj
```

Run this asynchronously (it stays open) and give it a moment to show the window.

## Step 2 – Capture a screenshot

Run the bundled script, passing the key presses needed to reach the state you want to see.
`-Keys Enter` advances `Welcome → WaitingToServe`, where both paddles are visible.

```powershell
powershell -ExecutionPolicy Bypass -File .github/skills/visual-verify/scripts/capture-window.ps1 -Keys Enter -Out paddle-check.png
```

Common sequences:

| Goal | `-Keys` value |
|------|----------------|
| Show paddles (waiting to serve) | `Enter` |
| Start a rally (ball moving) | `Enter,Enter` |
| Toggle collision hitboxes then show them | `Enter,F1` |

Supported key names: `Enter`, `Space`, `W`, `S`, `Up`, `Down`, `F1`, `Escape`.
The script prints `Foreground now game: True` when focus succeeded — if it says `False`,
re-run it (another window stole focus).

## Step 3 – Inspect the image

View the saved PNG (e.g. `paddle-check.png`) and confirm the visual state. For colour
checks, compare against the values set in `Game1.Initialize()` / drawn in `Game1.Draw()`.

## Step 4 – Clean up

Close the game and remove the temporary screenshot so it is not committed:

```powershell
Stop-Process -Name 'Pong' -ErrorAction SilentlyContinue
Remove-Item paddle-check.png -ErrorAction SilentlyContinue
```

## Notes

- This is a **manual visual check**, not a replacement for unit tests. Still add/adjust
  xUnit tests for any logic change and run the mandatory build + test gate.
- The script targets the process named `Pong` (the game exe). If you renamed the output
  assembly, pass `-ProcessName <name>`.
