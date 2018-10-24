using ngchat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ngchat.Services.OnlineStatus {
    public interface IOnlineStorage {
        Task<IEnumerable<UserContract>> GetOnlineUsersAsync(DateTime forTime);
        Task<bool> RegisterActivityAsync(UserContract pinged, DateTime time, string presenseContextId);

    }

}
