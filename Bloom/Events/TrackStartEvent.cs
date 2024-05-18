using System.Threading.Tasks;
using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Represents the event that is called when a track starts.
/// </summary>
/// <param name="args">The <see cref="TrackStartEventArgs"/> that contains the event data.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public delegate Task TrackStartEvent(TrackStartEventArgs args);
