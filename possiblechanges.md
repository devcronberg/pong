# Possible Changes

This file is a backlog of exercises and feature ideas for the PONG project. Each change should preserve the event-driven architecture, include focused tests for domain logic, and finish with:

```powershell
dotnet build .\Pong.csproj --no-incremental
dotnet test .\tests\Pong.Tests\Pong.Tests.csproj
```

The final build must report 0 warnings and 0 errors. Analyzer diagnostics should be fixed, not suppressed.

## Small changes

These tasks should normally fit in 15-30 minutes and touch one or two files.

### Change the colour theme

Give each paddle, the ball, centre line, background, and score an intentional colour. Keep the colours in one small palette type or a clearly named group of fields instead of scattering colour literals through `Draw()`.

**Acceptance criteria:** The game uses the new palette in every state, collision outlines remain easy to distinguish, and no game behaviour changes.

### Make gameplay values configurable

Move ball speed, paddle speed, paddle size, and winning score into one configuration type. Reject values that make the game unplayable, such as non-positive dimensions or speeds.

**Acceptance criteria:** Default gameplay is unchanged, invalid configuration is tested, and game objects receive only the values they need.

### Add a serve countdown

Show `3`, `2`, `1` before the ball launches instead of launching immediately after Enter.

**Acceptance criteria:** The countdown uses elapsed game time rather than blocking or sleeping, input remains responsive, and the ball is stationary until the countdown finishes.

### Increase speed after paddle hits

Increase the ball's speed slightly after each paddle collision while preserving its direction and applying a maximum speed.

**Acceptance criteria:** Speed increases predictably, never exceeds the limit, and collision events still fire once per hit.

### Add score reset

Add `ScoreBoard.Reset()` and tests that verify both scores return to zero. Decide whether reset should raise an event and document that contract.

**Acceptance criteria:** Both scores reset atomically, tests cover the event behaviour, and no score field is made publicly writable.

## Medium changes

These tasks typically affect several types and should include new tests.

### Add a Game Over screen

The first player to reach a configurable score wins. Show the winner and let Enter reset the game.

**Important:** `OnBallScored()` currently changes the state after `ScoreBoard.AddPoint()` raises `ScoreChanged`. Ensure that it does not overwrite a `GameOver` transition made by `OnScoreChanged()`.

**Acceptance criteria:** The correct winner is shown, no serve starts after the winning point, and restart resets score, ball, serve direction, and state.

**Useful skill:** `/add-screen`

### Add pause and resume

Press `P` during play to pause. Draw a pause screen and resume without moving the ball or paddles by the elapsed time spent paused.

**Acceptance criteria:** Input uses edge detection, pause only applies during play, and resuming cannot create a large physics step.

**Useful skill:** `/add-screen`

### Add sound effects

Play different sounds for wall hits, paddle hits, and scoring. Load assets through the MonoGame content pipeline and trigger playback from the existing event handlers.

**Acceptance criteria:** Assets load on all supported targets, content-managed resources are not disposed separately, and gameplay classes remain independent of audio.

**Useful skill:** `/add-sound-effect`

### Add visual feedback

Add a short ball trail or a small particle burst after paddle hits and scoring. Keep effect lifetime and update logic outside `Draw()`.

**Acceptance criteria:** Effects expire, do not grow an unbounded collection, reset correctly, and do not change collision bounds.

**Useful skill:** `/add-game-object`

### Add a basic computer opponent

Introduce an input abstraction such as `IPlayerController`. Implement a keyboard controller and a simple bot that moves one paddle towards the ball.

**Acceptance criteria:** `Paddle` no longer reads global keyboard state directly, human-vs-human still works, the bot cannot exceed paddle speed or leave the screen, and controller decisions can be unit-tested without opening a game window.

Avoid giving the bot direct write access to paddle position. It should provide intent, such as `-1`, `0`, or `1`, and let `Paddle` apply speed, elapsed time, and bounds.

## Larger changes

These are multi-step projects. Write a short design and define intermediate milestones before implementation.

### Add bot difficulty levels

Build on `IPlayerController` with easy, medium, and hard bots. Difficulty can vary reaction delay, targeting error, maximum speed, or whether the bot predicts the ball's future intersection with its side.

**Suggested milestones:** Extract controller input, implement a deterministic tracking bot, add prediction, then add controlled randomness through an injected random source.

**Acceptance criteria:** Each level has observable limits, bot logic is deterministic in tests, and difficulty does not bypass normal paddle physics.

### Control the game through an external API

Add a small local API or WebSocket service that can start, pause, reset, serve, or control a paddle. Keep networking outside the MonoGame classes.

Use a thread-safe command queue between the API and the game loop:

```text
HTTP/WebSocket request -> validate command -> enqueue command
Game1.Update()          -> dequeue command  -> change game state
```

Do not mutate MonoGame objects directly from an API request thread. MonoGame state should only change on the game thread during `Update()`.

**Suggested endpoints:**

- `GET /state` returns state, scores, and ball position
- `POST /commands/start` starts or serves
- `POST /commands/pause` pauses or resumes
- `POST /commands/reset` resets the match
- `POST /players/{player}/move` submits up, down, or stop intent

**Acceptance criteria:** Commands are validated, concurrent requests cannot corrupt game state, the service binds to localhost by default, and network exposure requires authentication. Test command parsing and queue handling independently of graphics.

### Add a browser or phone controller

Build a small web client on top of the external API. Use buttons or touch input for paddle movement and show connection status and the current score.

**Acceptance criteria:** Releasing a button sends stop intent, lost connections cannot leave a paddle moving forever, and the controller works on a local network only when explicitly enabled.

### Record and replay matches

Record timestamped input commands and important game events, then replay a match without live keyboard input.

**Acceptance criteria:** The replay format is versioned, parsing rejects invalid data, and deterministic inputs produce repeatable game-state snapshots. Random serve data must be recorded or generated from a stored seed.

### Add match telemetry

Track rally length, paddle hits, scores, ball speed, and match duration. Export a final summary as JSON or expose it through the external API.

**Acceptance criteria:** Telemetry listens to events where possible, does not change gameplay, resets between matches, and serialisation is covered by tests.

### Add network multiplayer

Allow players on two machines to join the same match. Define one authoritative host, exchange player intent rather than arbitrary positions, and plan for latency and disconnects.

**Suggested milestones:** Separate deterministic game state from rendering, define network messages, implement local client/server play, then add prediction or interpolation only if measurements show it is needed.

