using System.Threading;
using System.Threading.Tasks;

namespace VueServer.Classes
{
    public interface IScheduledTask
    {
        string Schedule { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}