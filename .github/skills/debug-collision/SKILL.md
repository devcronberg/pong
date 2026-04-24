---
name: debug-collision
description: "Visualise and debug collision boxes in this MonoGame project. Use when collision feels wrong, the ball passes through a paddle, or you want to draw hitboxes for any game object during development."
---

# Debug Collision Boxes

Use this skill to draw visible hitboxes around game objects so you can see exactly what the collision system detects.

## How collision works in this project

`Ball.Update()` uses `Rectangle.Intersects()` to check if the ball's bounds overlap a paddle's bounds. Both are returned by `GetBounds()` on `Ball` and `Paddle`.

## Step 1 – Add a debug flag

In `Types/Game1.cs`, add a toggle at the top of the class:

```csharp
private const bool DebugCollision = true; // set to false for release
```

## Step 2 – Draw hitboxes in Draw()

After drawing the normal game objects, add a debug-only block in `Game1.Draw()`:

```csharp
if (DebugCollision)
{
    DrawRectOutline(_ball.GetBounds(),    Color.Red);
    DrawRectOutline(_paddle1.GetBounds(), Color.Lime);
    DrawRectOutline(_paddle2.GetBounds(), Color.Lime);
}
```

## Step 3 – Add the outline helper

Add this helper method to `Game1.cs` alongside `DrawRect`:

```csharp
/// <summary>Draws a 1-pixel border around a rectangle for debug visualisation.</summary>
private void DrawRectOutline(Rectangle r, Color color)
{
    DrawRect(r.Left,        r.Top,          r.Width, 1,        color); // top
    DrawRect(r.Left,        r.Bottom - 1,   r.Width, 1,        color); // bottom
    DrawRect(r.Left,        r.Top,          1,       r.Height, color); // left
    DrawRect(r.Right - 1,   r.Top,          1,       r.Height, color); // right
}
```

## Step 4 – Run and inspect

Start the game with `dotnet run` or F5. Red = ball, Green = paddles. You can now see:
- Whether the hitboxes match the visual positions
- If the ball overlaps a paddle before the event fires
- The exact moment `Intersects()` becomes true

## Extending to other objects

Call `DrawRectOutline(myObject.GetBounds(), Color.Yellow)` for any object that has a `GetBounds()` method returning a `Rectangle`.

## Tip: toggle with a key

Instead of a compile-time constant, you can toggle debug mode at runtime:

```csharp
// in Update(), outside the state switch:
if (kb.IsKeyDown(Keys.F1) && !_prevKb.IsKeyDown(Keys.F1))
    _debugCollision = !_debugCollision;
```
