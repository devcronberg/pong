using System.Text;
using Game1;
using Xunit;

namespace Pong.Tests;

public class GameLogTests
{
  [Fact]
  public void Write_AppendsAcrossSessions_AndPreservesUtf8()
  {
    var path = Path.Combine(Path.GetTempPath(), $"pong-{Guid.NewGuid()}.log");
    try
    {
      new GameLog(path).Write("First session");
      new GameLog(path).Write("New session \u00e6\u00f8\u00e5 \u2013 ready");

      var lines = File.ReadAllLines(path, new UTF8Encoding(false, true));
      Assert.Equal(2, lines.Length);
      Assert.Matches(@"^\[\d{2}:\d{2}:\d{2} INF\] First session$", lines[0]);
      Assert.EndsWith("New session \u00e6\u00f8\u00e5 \u2013 ready", lines[1]);
    }
    finally
    {
      File.Delete(path);
    }
  }

  [Fact]
  public void Write_IsImmediatelyVisible_ToAnOpenReader()
  {
    var path = Path.Combine(Path.GetTempPath(), $"pong-{Guid.NewGuid()}.log");
    try
    {
      var log = new GameLog(path);
      log.Write("Game starting");
      using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
      using var reader = new StreamReader(stream, Encoding.UTF8);
      Assert.EndsWith("Game starting", reader.ReadLine());
      Assert.Null(reader.ReadLine());

      log.Write("Ball served towards Player 1");

      Assert.EndsWith("Ball served towards Player 1", reader.ReadLine());
      Assert.Null(reader.ReadLine());
    }
    finally
    {
      File.Delete(path);
    }
  }
}