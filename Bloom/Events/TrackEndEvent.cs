using System.Threading.Tasks;

namespace Bloom.Events;

/// <summary>
/// Represents the event that is called when a track ends.
/// </summary>
/// <param name="args">The <see cref="TrackEndEventArgs"/> that contains the event data.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public delegate Task TrackEndEvent(TrackEndEventArgs args);
