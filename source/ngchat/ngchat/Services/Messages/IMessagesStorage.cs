using ngchat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ngchat.Services.Messages {
    public interface IMessagesStorage {
        Task<IEnumerable<MessageContract>> GetHistoryAsync(DateTime from);
        Task<bool> SaveMessageAsync(MessageContract message);

    }

}
