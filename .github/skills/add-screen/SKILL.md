---
name: add-screen
description: "Add a new game screen or state to this MonoGame project. Use when adding a pause screen, game over screen, high score screen, or any new GameState. Guides through extending the GameState enum, drawing the screen, and wiring state transitions."
argument-hint: "Describe the screen, e.g. 'a game over screen that shows the winner and a restart option'"
---

# Add a New Game Screen

Follow these steps to add a new screen/state to the PONG game, consistent with the existing state machine in `Game1.cs`.

## How screens work in this project

The game uses a `GameState` enum (in `Types/GameState.cs`) to control what is shown and updated each frame. `Game1.Update()` and `Game1.Draw()` switch on `_state` to decide behaviour.

## Step 1 – Add the new state to the enum

Edit `Types/GameState.cs` and add the new value:

```csharp
public enum GameState
{
    Welcome,
    WaitingToServe,
    Playing,
    GameOver      // ← add your new state here
}
```

## Step 2 – Trigger the transition

In `Types/Game1.cs`, find the place where the transition should happen (e.g. after a score reaches a limit) and set `_state`:

```csharp
private void OnScoreChanged(object? sender, ScoreChangedEventArgs e)
{
    if (e.NewScore >= 5)
        _state = GameState.GameOver; // transition to the new screen
}
```

## Step 3 – Handle input on the new screen

In `Game1.Update()`, add a case for the new state:

```csharp
case GameState.GameOver:
    if (enterPressed)
    {
        // reset scores, ball, paddles and return to welcome
        _score1 = 0; // or reset via ScoreBoard
        _state = GameState.Welcome;
    }
    break;
```

## Step 4 – Draw the new screen

In `Game1.Draw()`, add a guard block before the gameplay drawing:

```csharp
if (_state == GameState.GameOver)
{
    DrawCentered("Game Over!", ScreenH / 2f - 50);
    DrawCentered("Press Enter to play again", ScreenH / 2f + 10);
    _spriteBatch.End();
    base.Draw(gameTime);
    return;
}
```

## Step 5 – Build and verify

Run `dotnet build` and confirm 0 errors.
