using System.Threading;
using System.Threading.Tasks;

namespace Blossom;

public interface IService
{
    public ValueTask InitializeAsync(CancellationToken cancellationToken = default);
}
