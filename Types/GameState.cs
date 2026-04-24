namespace Game1;

/// <summary>The high-level phase the game is in at any given moment.</summary>
public enum GameState
{
    Welcome,        // Title screen shown when the application starts
    WaitingToServe, // Ball is centred; waiting for Enter before play begins
    Playing         // Ball is in motion; paddles can be moved
}
