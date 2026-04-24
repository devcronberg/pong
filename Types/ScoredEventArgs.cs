using System;

namespace Game1;

/// <summary>Fired when the ball exits the left or right edge.</summary>
public class ScoredEventArgs : EventArgs
{
    /// <summary>The player number (1 or 2) who scored the point.</summary>
    public int Player { get; }

    public ScoredEventArgs(int player) => Player = player;
}
