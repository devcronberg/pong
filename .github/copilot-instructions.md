# GitHub Copilot Instructions

This is a MonoGame PONG project used for teaching OOP and event-driven programming in C#.

## Architecture

- `Pong.csproj` is the game project
- `tests/Pong.Tests/Pong.Tests.csproj` is the xUnit test project
- All game types live in `Types/` — one class per file
- `Game1.cs` is the **orchestrator only** — it creates objects, wires events, and delegates to them
- Objects communicate exclusively via **C# events** — no direct calls between `Ball`, `Paddle`, or `ScoreBoard`
- `Program.cs` is the entry point — keep it minimal

## Coding conventions

- **Namespace**: `Game1` throughout
- **Test namespace**: `Pong.Tests`
- **Nullable**: enabled — use `null!` for fields initialised in `Initialize()` or `LoadContent()`
- **One type per file** — new classes, enums, and EventArgs each get their own file in `Types/`
- **XML docs** on all public members
- **Comments in English**

## Testing

- Use xUnit and place tests in `tests/Pong.Tests/`
- Add or update tests when changing game logic
- Test domain logic and events independently of rendering
- Treat analyzer diagnostics as acceptance criteria; do not suppress them unless they are documented false positives
- Run `dotnet build Pong.csproj --no-incremental` and resolve every error and warning before running the tests
- Run `dotnet test tests/Pong.Tests/Pong.Tests.csproj` before completing a change
- Do not finish until the final build reports 0 warnings and 0 errors and all tests pass

## When adding a new game object

Follow the `/add-game-object` skill: create the class in `Types/`, define EventArgs in their own file, register and wire in `Game1.cs`.

## When adding a new screen

Follow the `/add-screen` skill: extend `GameState.cs`, handle in `Update()` switch, draw in `Draw()`.

## When adding sound

Follow the `/add-sound-effect` skill: add to `Content.mgcb`, load in `LoadContent()`, play from an event handler.

## Do not

- Add logic to `Program.cs`
- Put multiple types in one file
- Call methods directly between `Ball`, `Paddle`, and `ScoreBoard` — use events
- Add NuGet packages without a clear reason
- Put test files in the game project
