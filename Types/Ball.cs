using System;
using Microsoft.Xna.Framework;

namespace Game1;

// ---------------------------------------------------------------------------
// Ball class
// ---------------------------------------------------------------------------

/// <summary>
/// Represents the ball. Handles its own movement and collision detection,
/// then notifies the rest of the game via events.
/// </summary>
public class Ball
{
    // --- Constants ---
    public const float Size      = 12f;   // Width and height of the square ball
    public const float BaseSpeed = 320f;  // Speed applied when the ball is served

    // --- Properties ---

    /// <summary>Top-left position of the ball in screen pixels.</summary>
    public Vector2 Position { get; private set; }

    /// <summary>Current velocity in pixels per second.</summary>
    public Vector2 Velocity { get; private set; }

    // --- Events ---

    /// <summary>
    /// Raised when the ball passes the left or right screen edge.
    /// Subscribers should reset the ball and update the score.
    /// </summary>
    public event EventHandler<ScoredEventArgs>? Scored;

    /// <summary>Raised when the ball bounces off the top or bottom wall.</summary>
    public event EventHandler? WallHit;

    /// <summary>Raised when the ball bounces off one of the paddles.</summary>
    public event EventHandler<PaddleHitEventArgs>? PaddleHit;

    // --- Private fields ---
    private readonly float  _screenWidth;
    private readonly float  _screenHeight;
    private readonly Random _rng = new();

    // ---------------------------------------------------------------------------
    // Constructor
    // ---------------------------------------------------------------------------

    /// <param name="screenWidth">Width of the play area in pixels.</param>
    /// <param name="screenHeight">Height of the play area in pixels.</param>
    public Ball(float screenWidth, float screenHeight)
    {
        _screenWidth  = screenWidth;
        _screenHeight = screenHeight;
        PlaceAtCenter();
    }

    // ---------------------------------------------------------------------------
    // Public methods
    // ---------------------------------------------------------------------------

    /// <summary>Moves the ball to the screen centre and stops it.</summary>
    public void PlaceAtCenter()
    {
        Position = new Vector2(_screenWidth / 2f - Size / 2f, _screenHeight / 2f - Size / 2f);
        Velocity = Vector2.Zero; // Ball is stationary until Launch() is called
    }

    /// <summary>
    /// Launches the ball with a random angle towards the specified player.
    /// </summary>
    /// <param name="towardsPlayer">1 = launch left, 2 = launch right.</param>
    public void Launch(int towardsPlayer)
    {
        // Pick a random angle between 20° and 39° (avoids nearly-flat trajectories)
        float angle = MathHelper.ToRadians(20 + _rng.Next(0, 20));

        float dx = towardsPlayer == 1 ? -1f : 1f; // Horizontal direction
        float dy = _rng.Next(0, 2) == 0 ? 1f : -1f; // Random vertical direction

        // Decompose the angle into X and Y components, then scale to base speed
        Velocity = new Vector2(dx * MathF.Cos(angle), dy * MathF.Sin(angle)) * BaseSpeed;
    }

    /// <summary>Returns a Rectangle representing the ball's collision area.</summary>
    public Rectangle GetBounds() =>
        new Rectangle((int)Position.X, (int)Position.Y, (int)Size, (int)Size);

    // ---------------------------------------------------------------------------
    // Update – called once per frame while the ball is in motion
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Moves the ball and checks all collisions. Fires events when something happens.
    /// </summary>
    /// <param name="dt">Seconds elapsed since the last frame.</param>
    /// <param name="paddle1">The left paddle.</param>
    /// <param name="paddle2">The right paddle.</param>
    public void Update(float dt, Paddle paddle1, Paddle paddle2)
    {
        // --- Move the ball ---
        Position = Position + Velocity * dt;

        // --- Bounce off the top edge ---
        if (Position.Y <= 0)
        {
            Position = new Vector2(Position.X, 0);               // Clamp to edge
            Velocity = new Vector2(Velocity.X, MathF.Abs(Velocity.Y)); // Reflect downward
            WallHit?.Invoke(this, EventArgs.Empty);              // Notify listeners
        }

        // --- Bounce off the bottom edge ---
        if (Position.Y + Size >= _screenHeight)
        {
            Position = new Vector2(Position.X, _screenHeight - Size);    // Clamp to edge
            Velocity = new Vector2(Velocity.X, -MathF.Abs(Velocity.Y)); // Reflect upward
            WallHit?.Invoke(this, EventArgs.Empty);
        }

        var bounds = GetBounds();

        // --- Bounce off the left paddle (player 1) ---
        if (Velocity.X < 0 && bounds.Intersects(paddle1.GetBounds()))
        {
            // Push ball to the right of the paddle to prevent it from getting stuck
            Position = new Vector2(paddle1.GetBounds().Right, Position.Y);

            // 'rel' is how far from the paddle centre the ball struck.
            // Positive = below centre, negative = above. Used to add spin.
            float rel = (Position.Y + Size / 2f) - paddle1.CenterY;
            Velocity  = new Vector2(MathF.Abs(Velocity.X), rel / (Paddle.Height / 2f) * BaseSpeed);

            PaddleHit?.Invoke(this, new PaddleHitEventArgs(1));
        }

        // --- Bounce off the right paddle (player 2) ---
        if (Velocity.X > 0 && bounds.Intersects(paddle2.GetBounds()))
        {
            // Push ball to the left of the paddle
            Position = new Vector2(paddle2.GetBounds().Left - Size, Position.Y);

            float rel = (Position.Y + Size / 2f) - paddle2.CenterY;
            Velocity  = new Vector2(-MathF.Abs(Velocity.X), rel / (Paddle.Height / 2f) * BaseSpeed);

            PaddleHit?.Invoke(this, new PaddleHitEventArgs(2));
        }

        // --- Scoring: ball exits the left edge → player 2 scored ---
        if (Position.X + Size < 0)
            Scored?.Invoke(this, new ScoredEventArgs(2));

        // --- Scoring: ball exits the right edge → player 1 scored ---
        if (Position.X > _screenWidth)
            Scored?.Invoke(this, new ScoredEventArgs(1));
    }
}
