using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ngchat.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ngchat.Services.OnlineStatus {


    public class ServerOnlineNotificationTimer : BackgroundService {
        private const int NOTIFY_EVERY_MS = 1000;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ServerOnlineNotificationTimer(IServiceScopeFactory serviceScopeFactory) {
            _serviceScopeFactory = serviceScopeFactory;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                using (var scope = _serviceScopeFactory.CreateScope()) {
                    var onlineStorage = scope.ServiceProvider.GetService<IOnlineStorage>();
                    var hubContext = scope.ServiceProvider.GetService<IHubContext<CommonChatHub>>();

                    var onlineUsers = ( await onlineStorage.GetOnlineUsersAsync(DateTime.Now) ).Select(a => a.Username);

                    await hubContext.Clients.All.SendAsync("ReceiveOnlineList", onlineUsers);
                }
                await Task.Delay(NOTIFY_EVERY_MS);
            }
        }


    }
}
