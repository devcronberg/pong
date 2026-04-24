using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game1;

/// <summary>
/// Represents a player-controlled paddle.
/// The paddle reads its own keyboard input – the keys are injected via the constructor,
/// which is an example of the Dependency Injection principle.
/// </summary>
public class Paddle
{
    // --- Constants (shared by all paddles) ---
    public const float Width  = 14f;  // Paddle width in pixels
    public const float Height = 90f;  // Paddle height in pixels
    public const float Speed  = 380f; // Movement speed in pixels per second

    // --- Properties ---

    /// <summary>Top-left corner of the paddle in screen pixels.</summary>
    public Vector2 Position { get; private set; }

    /// <summary>Vertical centre of the paddle – used by Ball to calculate spin.</summary>
    public float CenterY => Position.Y + Height / 2f;

    // --- Private fields ---
    private readonly Keys  _upKey;       // Key that moves this paddle up
    private readonly Keys  _downKey;     // Key that moves this paddle down
    private readonly float _screenHeight; // Used to clamp movement to the screen

    // ---------------------------------------------------------------------------
    // Constructor
    // ---------------------------------------------------------------------------

    /// <param name="x">Horizontal position (left edge) of the paddle.</param>
    /// <param name="screenHeight">Height of the play area; used to clamp movement.</param>
    /// <param name="upKey">Key that moves the paddle upward.</param>
    /// <param name="downKey">Key that moves the paddle downward.</param>
    public Paddle(float x, float screenHeight, Keys upKey, Keys downKey)
    {
        _screenHeight = screenHeight;
        _upKey        = upKey;
        _downKey      = downKey;

        // Start the paddle centred vertically on its side of the screen
        Position = new Vector2(x, (screenHeight - Height) / 2f);
    }

    // ---------------------------------------------------------------------------
    // Public methods
    // ---------------------------------------------------------------------------

    /// <summary>Returns a Rectangle representing the paddle's collision area.</summary>
    public Rectangle GetBounds() =>
        new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);

    // ---------------------------------------------------------------------------
    // Update – called once per frame
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Reads keyboard input and moves the paddle, keeping it within the screen.
    /// </summary>
    /// <param name="dt">Seconds elapsed since the last frame.</param>
    public void Update(float dt)
    {
        var kb = Keyboard.GetState();
        float y = Position.Y;

        // Move up or down based on the keys assigned to this paddle
        if (kb.IsKeyDown(_upKey))   y -= Speed * dt;
        if (kb.IsKeyDown(_downKey)) y += Speed * dt;

        // Clamp so the paddle never leaves the screen vertically
        y = MathHelper.Clamp(y, 0, _screenHeight - Height);

        Position = new Vector2(Position.X, y);
    }
}
