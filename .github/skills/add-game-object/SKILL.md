---
name: add-game-object
description: "Add a new MonoGame game object (sprite, entity, power-up, obstacle, effect) to this project. Use when adding a new class that participates in the game loop with Update/Draw, fires events, or interacts with Ball, Paddle or ScoreBoard."
argument-hint: "Describe the game object to add, e.g. 'a power-up that speeds up the ball'"
---

# Add a MonoGame Game Object

Follow these steps to add a new game entity to the PONG project, consistent with the existing OOP and event-driven architecture.

## Project conventions to follow

- One class per file, placed in `Types/`
- Namespace: `Game1`
- Events use `EventArgs` subclasses in their own file in `Types/`
- `Game1.cs` is the orchestrator — new objects are created there and wired via events
- `nullable` is enabled — use `null!` for fields initialised in `LoadContent`/`Initialize`

## Step 1 – Define the class

Create `Types/<ClassName>.cs`:

```csharp
namespace Game1;

/// <summary>Describe what this object does.</summary>
public class MyObject
{
    // Constants
    public const float Width = 20f;

    // Properties
    public Vector2 Position { get; private set; }

    // Events (define EventArgs in a separate file if needed)
    public event EventHandler? SomethingHappened;

    public MyObject(/* inject dependencies */) { }

    public Rectangle GetBounds() =>
        new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Width);

    public void Update(float dt) { /* logic */ }
}
```

## Step 2 – Create EventArgs (if needed)

Create `Types/<EventName>EventArgs.cs` for any new events:

```csharp
using System;
namespace Game1;

public class MyEventArgs : EventArgs
{
    public int SomeValue { get; }
    public MyEventArgs(int value) => SomeValue = value;
}
```

## Step 3 – Register in Game1.cs

In `Types/Game1.cs`:

1. Add a private field: `private MyObject _myObject = null!;`
2. Instantiate in `Initialize()`: `_myObject = new MyObject(...);`
3. Wire events: `_myObject.SomethingHappened += OnSomethingHappened;`
4. Add handler method: `private void OnSomethingHappened(object? sender, MyEventArgs e) { }`
5. Call `_myObject.Update(dt)` in the `Playing` case in `Update()`
6. Call draw logic in `Draw()` using `DrawRect()` or `_spriteBatch.Draw()`

## Step 4 – Load content (if the object needs a texture or sound)

In `Content/Content.mgcb`, add:
```
#begin MySprite.png
/importer:TextureImporter
/processor:TextureProcessor
/build:MySprite.png
```

In `Game1.LoadContent()`:
```csharp
_myTexture = Content.Load<Texture2D>("MySprite");
```

## Step 5 – Build and verify

Run `dotnet build` and confirm 0 errors before testing.
