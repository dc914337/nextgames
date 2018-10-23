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

        public async Task GetMessageHistory() {

        }

        public override async Task OnConnectedAsync() {
            await base.OnConnectedAsync();
            
        }

        public override async Task OnDisconnectedAsync(Exception ex) {
            await base.OnDisconnectedAsync(ex);
            
        }
        
        private async Task NotifyNewMessage() {
            var usernames = Context.User.Identities.Select(a => a.Name.ToString()).ToList();
            string.Join(", ", usernames);
            await Clients.All.SendAsync("ReceiveConnected", usernames);
        }

        private async Task NotifyNewOnline() {
            var usernames = Context.User.Identities.Select(a => a.Name.ToString()).ToList();
            string.Join(", ", usernames);
            await Clients.All.SendAsync("ReceiveConnected", usernames);
        }


    }
}
