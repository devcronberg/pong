using Game1;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace Pong.Tests;

public class BallTests
{
  [Theory]
  [InlineData(800f, 600f, 394f, 294f)]
  [InlineData(1025f, 769f, 506.5f, 378.5f)]
  public void NewBall_StartsCenteredAndStationary(
      float screenWidth, float screenHeight, float expectedX, float expectedY)
  {
    var ball = new Ball(screenWidth, screenHeight);

    Assert.Equal(new Vector2(expectedX, expectedY), ball.Position);
    Assert.Equal(Vector2.Zero, ball.Velocity);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(2)]
  public void PlaceAtCenter_AfterMovement_CentersAndStopsBall(int towardsPlayer)
  {
    var ball = new Ball(800f, 600f);
    var leftPaddle = new Paddle(20f, 600f, Keys.W, Keys.S);
    var rightPaddle = new Paddle(766f, 600f, Keys.Up, Keys.Down);
    var center = new Vector2(394f, 294f);
    ball.Launch(towardsPlayer);
    ball.Update(0.1f, leftPaddle, rightPaddle);
    Assert.NotEqual(center, ball.Position);
    Assert.NotEqual(Vector2.Zero, ball.Velocity);

    ball.PlaceAtCenter();

    Assert.Equal(center, ball.Position);
    Assert.Equal(Vector2.Zero, ball.Velocity);
  }

  [Theory]
  [InlineData(1, -1)]
  [InlineData(2, 1)]
  public void Launch_ServesTowardsPlayerAtBaseSpeed(
      int towardsPlayer, int expectedHorizontalDirection)
  {
    var ball = new Ball(800f, 600f);
    var positionBeforeLaunch = ball.Position;

    ball.Launch(towardsPlayer);

    Assert.Equal(expectedHorizontalDirection, Math.Sign(ball.Velocity.X));
    Assert.InRange(ball.Velocity.Length(), Ball.BaseSpeed - 0.001f, Ball.BaseSpeed + 0.001f);
    Assert.NotEqual(0f, ball.Velocity.Y);
    Assert.Equal(positionBeforeLaunch, ball.Position);
  }
}