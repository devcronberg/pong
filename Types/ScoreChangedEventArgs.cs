using System;

namespace Game1;

/// <summary>Raised by ScoreBoard when a player's score increases.</summary>
public class ScoreChangedEventArgs : EventArgs
{
    /// <summary>The player whose score changed (1 or 2).</summary>
    public int Player { get; }

    /// <summary>The player's new score after the update.</summary>
    public int NewScore { get; }

    public ScoreChangedEventArgs(int player, int newScore)
    {
        Player   = player;
        NewScore = newScore;
    }
}
