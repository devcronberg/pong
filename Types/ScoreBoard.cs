using System;

namespace Game1;

// ---------------------------------------------------------------------------
// ScoreBoard class
// ---------------------------------------------------------------------------

/// <summary>
/// Tracks the scores for both players and fires an event whenever a score changes.
/// This separates score-keeping logic from rendering and game-state logic.
/// </summary>
public class ScoreBoard
{
    // --- Properties ---

    /// <summary>Current score for player 1 (left side).</summary>
    public int Score1 { get; private set; }

    /// <summary>Current score for player 2 (right side).</summary>
    public int Score2 { get; private set; }

    // --- Events ---

    /// <summary>
    /// Raised after a point is added to either player.
    /// Subscribers can react to score changes (e.g. check for a win condition).
    /// </summary>
    public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;

    // ---------------------------------------------------------------------------
    // Public methods
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Awards one point to the specified player and fires <see cref="ScoreChanged"/>.
    /// </summary>
    /// <param name="player">The player to award the point to (1 or 2).</param>
    public void AddPoint(int player)
    {
        if (player == 1)
        {
            Score1++;
            ScoreChanged?.Invoke(this, new ScoreChangedEventArgs(1, Score1));
        }
        else
        {
            Score2++;
            ScoreChanged?.Invoke(this, new ScoreChangedEventArgs(2, Score2));
        }
    }
}
