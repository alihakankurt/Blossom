using System.Threading.Tasks;

namespace Bloom.Events;

/// <summary>
/// Represents the event that is called when a track gets stuck.
/// </summary>
/// <param name="args">The <see cref="TrackStuckEventArgs"/> that contains the event data.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public delegate Task TrackStuckEvent(TrackStuckEventArgs args);
