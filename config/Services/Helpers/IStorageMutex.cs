using System;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.Config.Services.Helpers
{
    public interface IStorageMutex
    {
        Task<bool> EnterAsync(string collectionId, string key, TimeSpan timeout);

        Task LeaveAsync(string collectionId, string key);
    }
}
