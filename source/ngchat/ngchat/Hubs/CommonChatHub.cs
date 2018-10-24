using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ngchat.Services.Messages;
using ngchat.Models.ViewModels;

namespace ngchat.Hubs {
    public class CommonChatHub : Hub {
        private readonly string CHAT_ID = "mainChat";
        public IMessagesStorage MessagesStorage { get; }

        public CommonChatHub(IMessagesStorage messagesStorage) {
            MessagesStorage = messagesStorage;
        }

        public async Task SendMessage(string message) {
            var newMessage = new Models.MessageContract {
                Message = message,
                Created = DateTime.Now,
                ChatId = CHAT_ID,
                Sender = new Models.UserContract {
                    Username = Context.User.Identity.Name,
                    UserGUID = Context.UserIdentifier,
                }
            };
            if (await MessagesStorage.SaveMessageAsync(newMessage)) {
                await NotifyNewMessage(newMessage);
            }
        }

        public async Task<ICollection<WallMessage>> GetMessageHistory(DateTime from) {
            var utcStartFrom = from.ToUniversalTime();
            var messages = await MessagesStorage.GetHistoryAsync(utcStartFrom);
            return messages.Select(a => new WallMessage() { Username = a.Sender.Username, Message = a.Message }).ToList();
        }

        private async Task NotifyNewMessage(Models.MessageContract newMessage) {
            await Clients.All.SendAsync("ReceiveMessage", newMessage.Sender.Username, newMessage.Message);
        }


        public override async Task OnConnectedAsync() {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex) {
            await base.OnDisconnectedAsync(ex); //check that it is guaranteed// timeout
        }

        private async Task NotifyNewOnline() {
            var usernames = Context.User.Identities.Select(a => a.Name.ToString()).ToList();
            string.Join(", ", usernames);
            await Clients.All.SendAsync("ReceiveConnected", usernames);
        }


    }
}
