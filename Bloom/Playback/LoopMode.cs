namespace Bloom.Playback;

/// <summary>
/// Represents the loop mode of the playback.
/// </summary>
public enum LoopMode
{
    /// <summary>
    /// No loop.
    /// </summary>
    None,

    /// <summary>
    /// Loops the current track.
    /// </summary>
    One,

    /// <summary>
    /// Loops the queue.
    /// </summary>
    All
}

/// <summary>
/// Provides extension methods for <see cref="LoopMode"/>.
/// </summary>
public static class LoopModeExtensions
{
    /// <summary>
    /// Gets the next loop mode.
    /// </summary>
    /// <param name="mode">The current loop mode.</param>
    /// <returns>The next loop mode.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static LoopMode Next(this LoopMode mode) => mode switch
    {
        LoopMode.None => LoopMode.One,
        LoopMode.One => LoopMode.All,
        LoopMode.All => LoopMode.None,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };
}
