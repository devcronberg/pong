# PONG — MonoGame OOP Teaching Project

A classic two-player PONG game built with **MonoGame DesktopGL** on **.NET 10**, designed as a teaching example for object-oriented programming and event-driven architecture in C#.

![](pong.png)

## Gameplay

| Player           | Up  | Down |
| ---------------- | --- | ---- |
| Player 1 (left)  | `W` | `S`  |
| Player 2 (right) | `↑` | `↓`  |

Press **Enter** to start and to serve. Press **Escape** to quit.

## Project structure

```
Pong.csproj
Program.cs              ← Entry point, creates and runs the game
Types/
  Game1.cs              ← Orchestrator: creates objects, wires events, calls Update/Draw
  Ball.cs               ← Ball physics, collision detection, fires events
  Paddle.cs             ← Paddle input handling and movement
  ScoreBoard.cs         ← Score tracking, fires ScoreChanged
  GameState.cs          ← Enum: Welcome | WaitingToServe | Playing
  ScoredEventArgs.cs    ← EventArgs for Ball.Scored
  PaddleHitEventArgs.cs ← EventArgs for Ball.PaddleHit
  ScoreChangedEventArgs.cs ← EventArgs for ScoreBoard.ScoreChanged
Content/
  Font.spritefont       ← Bitmap font (Arial 32pt) built by content pipeline
  Content.mgcb          ← MonoGame content pipeline build file
tests/
  Pong.Tests/
    Pong.Tests.csproj   ← xUnit test project
    ScoreBoardTests.cs  ← Tests for scoring logic and events
```

## OOP concepts demonstrated

| Concept                      | Where                                                                                        |
| ---------------------------- | -------------------------------------------------------------------------------------------- |
| **Encapsulation**            | Each class owns its data — only exposes what others need                                     |
| **Single Responsibility**    | `Ball` handles physics, `Paddle` handles input, `ScoreBoard` tracks points                   |
| **Events & Delegates**       | Objects communicate via events — no tight coupling                                           |
| **EventArgs subclasses**     | Strongly typed event data (`ScoredEventArgs`, `PaddleHitEventArgs`, `ScoreChangedEventArgs`) |
| **Dependency Injection**     | `Paddle` receives its keys via constructor                                                   |
| **Nullable reference types** | Enabled project-wide — fields are `null!` until initialised in `Initialize()`                |

## Event flow

```
Ball ──────→ Scored          → Game1.OnBallScored  → ScoreBoard.AddPoint()
        ──→ PaddleHit        → Game1.OnPaddleHit   → (extension point: sound)
        ──→ WallHit          → Game1.OnWallHit      → (extension point: sound)
ScoreBoard → ScoreChanged    → Game1.OnScoreChanged → (extension point: win condition)
```

## Getting started

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```powershell
# Run in development
dotnet run

# Run the tests
dotnet test .\tests\Pong.Tests\Pong.Tests.csproj

# Debug in VS Code
# Press F5 — uses .vscode/launch.json
```

### Live log viewer (Windows)

Run `powershell -ExecutionPolicy Bypass -File .\run.ps1` to start the game
with a separate UTF-8 log viewer. It shows the last 20 lines and follows new
events, including startup, serves, scores and exit. Older sessions remain in
the file; leave the viewer running to see new events.

The script sets `PONG_LOG_PATH` to the project's `game.log` for the game process.
Without this variable (for example with F5 or `dotnet run`), the game writes
`game.log` next to its executable, normally in `bin/Debug/net10.0/`.

## Testing

The xUnit suite covers scoring, score-change events, player-number validation,
file logging, and the ball's initial position, reset after movement, serve
direction and speed. Ball tests run without a game window and check properties
that hold for every random serve angle. Collision regression tests and tests for
new Game Over behaviour are left as course exercises. Rendering and the MonoGame
window are verified manually.

## Ideas and exercises

See [possiblechanges.md](possiblechanges.md) for small visual changes, game features, bots, external API control, and larger project ideas with acceptance criteria.

## Static analysis

[`IDisposableAnalyzers`](https://github.com/DotNetAnalyzers/IDisposableAnalyzers) runs during every build. The `IDISP003` and `IDISP006` diagnostics are treated as errors, so owned resources must be disposed correctly before the project and tests can build.

The repository must always build with **0 warnings and 0 errors**. Analyzer diagnostics should be fixed rather than suppressed.

```powershell
dotnet build .\Pong.csproj --no-incremental
```

### CI quality gate

Pull requests targeting `main` or `kursus*`, and pushes to those branches, run the **Build, Analyzers & Tests**
check on Windows. It builds the game and test project in Release with analyzers
enabled and all warnings treated as errors, then runs the tests without rebuilding.
TRX test reports are uploaded even when tests fail.

Publishing for all four platforms and creating a release only run on pushes to
`main`, after validation succeeds. Publish builds also treat warnings as errors.
To block merging a failing pull request, make **Build, Analyzers & Tests** a required
status check in GitHub branch protection or a repository ruleset.

### Course branches

Keep `main` as the maintained starting point. Before a course, create a branch
such as `kursus20260921` from `main`. During the course, target exercise pull
requests at that course branch, not `main`. CI validates these pull requests and
subsequent pushes, but course branches do not publish releases.

Use a separate protection rule matching `kursus*` to require the same CI check.
Allow deletion of course branches so they can be removed after participants have
saved their work. Keep deletion of `main` blocked. Course changes are not merged
back to `main` automatically; select reusable improvements separately.

## Publishing

```powershell
# Builds self-contained executables for Windows, Linux, and macOS
powershell -ExecutionPolicy Bypass -File .\publish.ps1
```

Output:
```
publish\win-x64\Pong.exe        Windows x64
publish\linux-x64\Pong          Linux x64
publish\osx-x64\Pong            macOS Intel
publish\osx-arm64\Pong          macOS Apple Silicon
```

## AI assistance (GitHub Copilot)

VS Code with GitHub Copilot is the default setup. The original instructions,
agent and skills live in `.github/` and are the only maintained versions.

|             | Name                | Use                                                      |
| ----------- | ------------------- | -------------------------------------------------------- |
| 🤖 **Agent** | `monogame-dev`      | MonoGame API help, game loop questions, content pipeline |
| 🛠 **Skill** | `/add-game-object`  | Add a new entity following the OOP/event pattern         |
| 🛠 **Skill** | `/add-screen`       | Add a new game screen (Game Over, Pause, etc.)           |
| 🛠 **Skill** | `/add-sound-effect` | Add audio via the content pipeline                       |
| 🛠 **Skill** | `/debug-collision`  | Draw hitboxes to visualise collision detection           |

### Serena MCP (optional, Windows)

Run the setup once after cloning, using Windows PowerShell 5.1 or PowerShell 7:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\install-serena.ps1
```

The script downloads and runs Astral's official uv installer if `uvx` is missing,
adds its directory to the user PATH when needed, and prepares the Python and
Serena versions configured in `.vscode/mcp.json`. It needs internet access but
does not require administrator rights or change the system execution policy.
Downloads and tool environments are stored outside the repository. Rerunning
the script reuses the installation and cache; rerun after a configured version
change or if the cache has been cleared.

Build or refresh the local symbol index separately:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\index-serena.ps1
```

Run this after setup, before a demo, or to refresh the cache after larger code
changes. The script uses the same pinned version as MCP and works regardless of
the current directory. It requires the .NET SDK and may download C# language-server
components on first use. Stop the Serena MCP server before running it to avoid
concurrent cache writes.

The shared `.serena/project.yml` selects C# and follows `.gitignore`, with explicit
exclusions for build/publish output, test results, logs, and generated Claude files.
Game and test source files remain included. Only `.serena/project.yml` is shared;
Serena cache, logs, and memories stay local and are ignored by Git. Indexing failures,
including partial failures, make the script fail instead of reporting success.

After the first installation, close all VS Code windows and reopen the project
to pick up PATH changes. Run **MCP: List Servers**, select **serena**, and
start/approve the server. The setup checks the CLI with `--help`; it does not
verify the MCP connection or C# symbol lookup. First server startup may download
language-server components and create local Serena configuration/cache files.
Serena is not required to build, test, or play PONG.

### Claude Code (optional)

Students using Claude Code can generate its configuration with Windows PowerShell
5.1 or PowerShell 7, without installing extra modules:

```powershell
powershell -ExecutionPolicy Bypass -File .\generate-claude.ps1
```

For PowerShell 7, use `pwsh -File ./generate-claude.ps1` instead.
Start a new Claude Code session in the project after generation.

| Copilot source                                 | Generated Claude Code output                         |
| ---------------------------------------------- | ---------------------------------------------------- |
| `.github/copilot-instructions.md`              | `CLAUDE.md`                                          |
| `.github/skills/` (including supporting files) | `.claude/skills/`                                    |
| `.github/agents/*.agent.md`                    | `.claude/agents/*.md` with translated metadata/tools |

Rerun the script after updating the originals, including after a pull. Generated
outputs are ignored by Git. Do not edit them: regeneration replaces `CLAUDE.md`
and the entire generated skills/agents directories, removing obsolete entries.
Other Claude files, such as `.claude/settings.local.json`, are left untouched.
The first run refuses to overwrite existing configuration at the output paths.

Agent conversion supports the current two-field frontmatter format (double-quoted
description and inline tool list). Unsupported formats or tool groups stop
generation before existing outputs are replaced. VS Code-specific settings and
workflows are not converted.

Run the generator's checks with:

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\Test-ClaudeGeneration.ps1
```

