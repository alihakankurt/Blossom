using System.Threading.Tasks;
using Bloom.Playback;

namespace Bloom.Events;

/// <summary>
/// Represents the event that is called when an exception is thrown.
/// </summary>
/// <param name="args">The <see cref="TrackExceptionEventArgs"/> that contains the event data.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public delegate Task TrackExceptionEvent(TrackExceptionEventArgs args);
