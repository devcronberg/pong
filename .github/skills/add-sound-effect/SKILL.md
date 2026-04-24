---
name: add-sound-effect
description: "Add a sound effect to this MonoGame project. Use when adding audio for paddle hits, wall bounces, scoring, or any game event. Guides through the content pipeline, loading the asset, and playing it from an event handler."
argument-hint: "Describe the sound, e.g. 'a beep when the ball hits a paddle'"
---

# Add a Sound Effect

Follow these steps to add a sound effect to the PONG game using the MonoGame content pipeline.

## Supported formats

MonoGame accepts `.wav`, `.mp3`, and `.ogg` for sound effects. `.wav` is recommended for short game sounds (low latency, no decoding overhead).

## Step 1 – Add the audio file

Place your sound file in `Content/`, e.g. `Content/Sounds/paddle-hit.wav`.

## Step 2 – Register it in the content pipeline

Edit `Content/Content.mgcb` and add:

```
#begin Sounds/paddle-hit.wav
/importer:WavImporter
/processor:SoundEffectProcessor
/processorParam:Quality=Best
/build:Sounds/paddle-hit.wav
```

For `.mp3` or `.ogg`, use `Mp3Importer` / `OggImporter` instead.

## Step 3 – Load the asset in Game1

In `Types/Game1.cs`, add a field:

```csharp
private SoundEffect _paddleHitSound = null!;
```

In `LoadContent()`:

```csharp
_paddleHitSound = Content.Load<SoundEffect>("Sounds/paddle-hit");
```

## Step 4 – Play it from the right event handler

Sound should be played in the event handler that matches the game event. For a paddle hit:

```csharp
private void OnPaddleHit(object? sender, PaddleHitEventArgs e)
{
    _paddleHitSound.Play(); // plays at full volume, no pitch change
}
```

To control volume or pitch:

```csharp
_paddleHitSound.Play(volume: 0.8f, pitch: 0f, pan: 0f);
```

## Step 5 – Build and verify

Run `dotnet build`. If the content pipeline fails to find the file, double-check the path in `.mgcb` matches the actual file location under `Content/`.

## Common issues

| Problem | Fix |
|---|---|
| `ContentLoadException` at runtime | Path in `Content.Load<>()` must not include `Content/` prefix or file extension |
| No sound on Linux | Ensure `libopenal` is installed (`sudo apt install libopenal-dev`) |
| Crackling on `.mp3` | Switch to `.wav` for short effects |
