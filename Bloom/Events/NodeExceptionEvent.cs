using System.Threading.Tasks;

namespace Bloom.Events;

/// <summary>
/// Represents the event that is called when a <see cref="BloomNode"/> throws an exception.
/// </summary>
/// <param name="args">The <see cref="NodeExceptionEventArgs"/> that contains the event data.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
public delegate Task NodeExceptionEvent(NodeExceptionEventArgs args);
