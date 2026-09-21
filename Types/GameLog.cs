using System;
using System.IO;
using System.Text;

namespace Game1;

/// <summary>Appends game events to a UTF-8 file without buffering between calls.</summary>
public sealed class GameLog
{
    private readonly string _path;

    /// <summary>Creates a logger that writes to the specified file.</summary>
    public GameLog(string path)
    {
        _path = path;
    }

    /// <summary>Appends a timestamped event and closes the file so readers see it immediately.</summary>
    public void Write(string message)
    {
        File.AppendAllText(_path, $"[{DateTime.Now:HH:mm:ss} INF] {message}{Environment.NewLine}",
            new UTF8Encoding(false));
    }
}