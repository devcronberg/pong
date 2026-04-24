using System;
using Microsoft.Xna.Framework;           // Core MonoGame types
using Microsoft.Xna.Framework.Graphics;  // SpriteBatch, Texture2D, SpriteFont
using Microsoft.Xna.Framework.Input;     // Keyboard input

namespace Game1;

// ---------------------------------------------------------------------------
// Game1 – the orchestrator
// ---------------------------------------------------------------------------

/// <summary>
/// Top-level MonoGame class. Its only responsibility is to:
///   1. Create the game objects (Ball, Paddle, ScoreBoard).
///   2. Wire up events so objects can communicate without tight coupling.
///   3. Forward MonoGame's Update/Draw calls to the right objects.
///   4. Handle the overall GameState machine (Welcome ? WaitingToServe ? Playing).
/// All game logic lives in Ball, Paddle, and ScoreBoard.
/// </summary>
public class Game1 : Game
{
    // --- MonoGame infrastructure ---
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!; // Initialised in LoadContent
    private Texture2D   _pixel       = null!; // 1×1 white texture used to draw rectangles
    private SpriteFont  _font        = null!; // Bitmap font for scores and UI text

    // --- Screen dimensions (constant throughout the session) ---
    private const int   ScreenW       = 800;
    private const int   ScreenH       = 600;
    private const float PaddleMargin  = 20f; // Gap between paddle and the screen edge

    // --- Game objects ---
    private Ball       _ball       = null!;
    private Paddle     _paddle1    = null!; // Left paddle  (player 1)
    private Paddle     _paddle2    = null!; // Right paddle (player 2)
    private ScoreBoard _scoreBoard = null!;

    // --- State machine ---
    private GameState    _state        = GameState.Welcome;
    private int          _serveTowards = 1;   // Which player the next ball is served towards
    private KeyboardState _prevKb;            // Last frame's keyboard state (for edge detection)

    // --- Debug ---
    private bool _debugCollision = false; // Toggle with F1 at runtime

    // ---------------------------------------------------------------------------
    // Constructor
    // ---------------------------------------------------------------------------

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth  = ScreenW;
        _graphics.PreferredBackBufferHeight = ScreenH;
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    // ---------------------------------------------------------------------------
    // MonoGame lifecycle
    // ---------------------------------------------------------------------------

    protected override void Initialize()
    {
        // --- Create game objects ---
        _ball      = new Ball(ScreenW, ScreenH);
        _paddle1   = new Paddle(PaddleMargin,                          ScreenH, Keys.W,  Keys.S);
        _paddle2   = new Paddle(ScreenW - PaddleMargin - Paddle.Width, ScreenH, Keys.Up, Keys.Down);
        _scoreBoard = new ScoreBoard();

        // --- Wire up events (loose coupling between objects) ---
        // Ball notifies Game1 when the ball scores, hits a paddle, or hits a wall.
        // Game1 then decides what to do (update score, play sound, etc.).
        _ball.Scored    += OnBallScored;
        _ball.PaddleHit += OnPaddleHit;
        _ball.WallHit   += OnWallHit;

        // ScoreBoard notifies Game1 whenever a score changes.
        _scoreBoard.ScoreChanged += OnScoreChanged;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1×1 white pixel texture; scaled to any size to draw rectangles
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _font = Content.Load<SpriteFont>("Font");
    }

    // ---------------------------------------------------------------------------
    // Event handlers – this is where the objects "talk" to each other
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Called by Ball when it exits the left or right edge.
    /// Responsibility: award the point, reset the ball, and wait for the next serve.
    /// </summary>
    private void OnBallScored(object? sender, ScoredEventArgs e)
    {
        _scoreBoard.AddPoint(e.Player); // Delegate score tracking to ScoreBoard
        _serveTowards = e.Player;       // Next serve goes towards the player who just scored
        _ball.PlaceAtCenter();          // Park the ball in the middle
        _state = GameState.WaitingToServe;
    }

    /// <summary>
    /// Called by Ball when it bounces off a paddle.
    /// This is a good place to play a sound or trigger a visual effect.
    /// </summary>
    private void OnPaddleHit(object? sender, PaddleHitEventArgs e)
    {
        // Example extension point: soundEffects.PaddleHit.Play();
    }

    /// <summary>
    /// Called by Ball when it bounces off the top or bottom wall.
    /// This is a good place to play a wall-bounce sound.
    /// </summary>
    private void OnWallHit(object? sender, EventArgs e)
    {
        // Example extension point: soundEffects.WallHit.Play();
    }

    /// <summary>
    /// Called by ScoreBoard whenever a player's score increases.
    /// This is a good place to check for a win condition.
    /// </summary>
    private void OnScoreChanged(object? sender, ScoreChangedEventArgs e)
    {
        // Example extension point: if (e.NewScore >= 10) _state = GameState.GameOver;
    }

    // ---------------------------------------------------------------------------
    // Update – called every frame
    // ---------------------------------------------------------------------------

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();

        // Escape quits regardless of game state
        if (kb.IsKeyDown(Keys.Escape))
            Exit();

        // F1 toggles collision hitbox visualisation
        if (kb.IsKeyDown(Keys.F1) && !_prevKb.IsKeyDown(Keys.F1))
            _debugCollision = !_debugCollision;

        // Detect a fresh Enter press (not a held-down press from the previous frame)
        bool enterPressed = kb.IsKeyDown(Keys.Enter) && !_prevKb.IsKeyDown(Keys.Enter);

        switch (_state)
        {
            case GameState.Welcome:
                // Wait for Enter, then move to the serve-ready state
                if (enterPressed)
                {
                    _serveTowards = 1;
                    _state = GameState.WaitingToServe;
                }
                break;

            case GameState.WaitingToServe:
                // Wait for Enter, then launch the ball
                if (enterPressed)
                {
                    _ball.Launch(_serveTowards);
                    _state = GameState.Playing;
                }
                break;

            case GameState.Playing:
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Delegate movement to each paddle (they read their own keys)
                _paddle1.Update(dt);
                _paddle2.Update(dt);

                // Delegate physics and collision to the ball
                // (Ball fires events back to us if something significant happens)
                _ball.Update(dt, _paddle1, _paddle2);
                break;
        }

        _prevKb = kb; // Save state for next frame's edge detection
        base.Update(gameTime);
    }

    // ---------------------------------------------------------------------------
    // Draw – called every frame after Update
    // ---------------------------------------------------------------------------

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();

        // --- Welcome screen ---
        if (_state == GameState.Welcome)
        {
            DrawCentered("Welcome to PONG",      ScreenH / 2f - 50);
            DrawCentered("Press Enter to start", ScreenH / 2f + 10);
            _spriteBatch.End();
            base.Draw(gameTime);
            return;
        }

        // --- Dashed centre line ---
        for (int y = 0; y < ScreenH; y += 30)
            DrawRect(ScreenW / 2 - 2, y, 4, 18, Color.DimGray); // Each dash: 4×18 px

        // --- Paddles ---
        var p1 = _paddle1.GetBounds();
        var p2 = _paddle2.GetBounds();
        DrawRect(p1.X, p1.Y, p1.Width, p1.Height, Color.White);
        DrawRect(p2.X, p2.Y, p2.Width, p2.Height, Color.White);

        // --- Ball ---
        var b = _ball.GetBounds();
        DrawRect(b.X, b.Y, b.Width, b.Height, Color.White);

        // --- Scores (centred in each player's half) ---
        string s1 = _scoreBoard.Score1.ToString();
        string s2 = _scoreBoard.Score2.ToString();
        _spriteBatch.DrawString(_font, s1, new Vector2(ScreenW / 4f       - _font.MeasureString(s1).X / 2f, 20), Color.White);
        _spriteBatch.DrawString(_font, s2, new Vector2(3 * ScreenW / 4f   - _font.MeasureString(s2).X / 2f, 20), Color.White);

        // --- Serve prompt (shown while waiting for the next serve) ---
        if (_state == GameState.WaitingToServe)
            DrawCentered("Press Enter to serve", ScreenH / 2f + 40);

        // --- Debug: collision hitboxes (toggle with F1) ---
        if (_debugCollision)
        {
            DrawRectOutline(_ball.GetBounds(),    Color.Red);    // Ball hitbox
            DrawRectOutline(_paddle1.GetBounds(), Color.Red);    // Player 1 paddle
            DrawRectOutline(_paddle2.GetBounds(), Color.Red);    // Player 2 paddle
            DrawRect(0, 0,           ScreenW, 2, Color.Red);     // Top wall
            DrawRect(0, ScreenH - 2, ScreenW, 2, Color.Red);     // Bottom wall
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    // ---------------------------------------------------------------------------
    // Private drawing helpers
    // ---------------------------------------------------------------------------

    /// <summary>Draws a string horizontally centred on the screen at the given Y coordinate.</summary>
    private void DrawCentered(string text, float y)
    {
        var size = _font.MeasureString(text);
        _spriteBatch.DrawString(_font, text, new Vector2((ScreenW - size.X) / 2f, y), Color.White);
    }

    /// <summary>Draws a filled rectangle by scaling the 1×1 pixel texture.</summary>
    private void DrawRect(int x, int y, int w, int h, Color color) =>
        _spriteBatch.Draw(_pixel, new Rectangle(x, y, w, h), color);

    /// <summary>Draws a 1-pixel border around a rectangle for debug visualisation.</summary>
    private void DrawRectOutline(Rectangle r, Color color)
    {
        DrawRect(r.Left,      r.Top,        r.Width, 1,        color); // top
        DrawRect(r.Left,      r.Bottom - 1, r.Width, 1,        color); // bottom
        DrawRect(r.Left,      r.Top,        1,       r.Height, color); // left
        DrawRect(r.Right - 1, r.Top,        1,       r.Height, color); // right
    }
}

