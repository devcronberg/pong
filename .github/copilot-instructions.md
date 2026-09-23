# GitHub Copilot Instructions

This is a MonoGame PONG project used for teaching OOP and event-driven programming in C#.

## Mandatory completion gate: zero errors, zero warnings, all tests passing

**AI agents MUST run and verify both build and tests after the final edit for every change, including documentation and configuration changes. Never declare a change complete without passing both checks.**

1. Run `dotnet build Pong.csproj --no-incremental`. The final build MUST succeed with **0 errors and 0 warnings**. Resolve all compiler and analyzer diagnostics before running tests.
2. Run `dotnet test tests/Pong.Tests/Pong.Tests.csproj`. The full test suite MUST succeed with **0 failed tests** and no build or test execution errors or warnings. Do not filter out, skip, or disable failing tests to obtain a passing result.
3. If either check fails, fix the cause and rerun both commands after the last edit. Earlier successful runs do not validate later edits.
4. Report the final build and test results in the response. If a check cannot run or a failure cannot be resolved within the task's scope, explicitly report the blocker and state that the change is **not fully verified**; do not claim completion.

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
- Always follow the mandatory completion gate above

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
