using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace ngchat.Hubs {
    public class CommonChatHub : Hub {
        public async Task SendMessage(string message) {
            var username = Context.User.Identity.Name;
            await Clients.All.SendAsync("ReceiveMessage", username, message);
        }
    }
}
