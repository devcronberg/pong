using System;

namespace Game1;

/// <summary>Fired when the ball bounces off a paddle.</summary>
public class PaddleHitEventArgs : EventArgs
{
    /// <summary>Which paddle was hit (1 = left, 2 = right).</summary>
    public int PaddleNumber { get; }

    public PaddleHitEventArgs(int paddleNumber) => PaddleNumber = paddleNumber;
}
