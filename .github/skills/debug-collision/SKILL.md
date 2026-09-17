---
name: debug-collision
description: "Visualise and debug collision boxes in this MonoGame project. Use when collision feels wrong, the ball passes through a paddle, an event fires unexpectedly, or collision visualisation must include a new object."
---

# Debug Collision Boxes

Use the project's existing collision visualisation before changing collision code.

## Existing support

- Press `F1` to toggle collision visualisation at runtime
- `Game1.Update()` performs edge detection so holding `F1` only toggles once
- `Game1.Draw()` draws the ball, paddle, and wall hitboxes in red when `_debugCollision` is enabled
- `DrawRectOutline()` renders rectangle boundaries

Do not add another debug flag, key handler, or outline helper.

## Step 1 – Reproduce and observe

Run the game, press `F1`, and reproduce the collision. Record which visible bounds overlap when the unexpected behaviour occurs.

## Step 2 – Inspect the collision path

- `Ball.GetBounds()` and `Paddle.GetBounds()` define the collision rectangles
- `Ball.Update()` moves the ball before checking walls, paddles, and scoring
- Paddle collision depends on both `Rectangle.Intersects()` and the sign of `Velocity.X`
- Collision events are handled by `Game1`

Identify which condition disagrees with the visualised bounds before editing code.

## Step 3 – Extend visualisation for new objects

Add the object's bounds to the existing `_debugCollision` block:

```csharp
DrawRectOutline(myObject.GetBounds(), Color.Yellow);
```

Use a distinct colour when overlapping rectangles need to be distinguished.

## Step 4 – Verify

Add or update focused tests for any changed collision logic, then run:

```powershell
dotnet build Pong.csproj --no-incremental
dotnet test tests/Pong.Tests/Pong.Tests.csproj
```

Do not finish until the build reports 0 warnings and 0 errors, all tests pass, and the collision behaviour has been checked with `F1`.
