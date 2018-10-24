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
using ngchat.Services.OnlineStatus;

namespace ngchat.Hubs {
    public class CommonChatHub : Hub {
        private readonly string CHAT_ID = "mainChat";
        public IMessagesStorage MessagesStorage { get; }
        public IOnlineStorage OnlineStorage { get; }

        public CommonChatHub(IMessagesStorage messagesStorage, IOnlineStorage onlineStorage) {
            MessagesStorage = messagesStorage;
            OnlineStorage = onlineStorage;
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
            await Ping();
        }

        public async Task<ICollection<WallMessage>> GetMessageHistory(DateTime from) {
            var messages = await MessagesStorage.GetHistoryAsync(from);
            return messages.Select(a => new WallMessage() { Username = a.Sender.Username, Message = a.Message }).ToList();
        }

        private async Task NotifyNewMessage(Models.MessageContract newMessage) {
            await Clients.All.SendAsync("ReceiveMessage", newMessage.Sender.Username, newMessage.Message);
        }

        public async Task Ping() {
            await OnlineStorage.RegisterActivityAsync(new Models.UserContract() {
                Username = Context.User.Identity.Name,
                UserGUID = Context.UserIdentifier
            },
           DateTime.Now,
           CHAT_ID
           );
        }

        public override async Task OnConnectedAsync() {
            await OnlineStorage.SetUserConnectedAsync(new Models.UserContract() {
                Username = Context.User.Identity.Name,
                UserGUID = Context.UserIdentifier
            }, DateTime.Now, CHAT_ID);
            await NotifyOnlineChanged();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex) {
            await OnlineStorage.SetUserDisconnectedAsync(new Models.UserContract() {
                Username = Context.User.Identity.Name,
                UserGUID = Context.UserIdentifier
            }, DateTime.Now, CHAT_ID);
            await NotifyOnlineChanged();
            await base.OnConnectedAsync();
        }


        private async Task NotifyOnlineChanged() {
            var usernames = ( await OnlineStorage.GetOnlineUsersAsync(DateTime.Now) ).Select(a => a.Username);
            await Clients.All.SendAsync("ReceiveOnlineList", usernames);
        }


    }
}
