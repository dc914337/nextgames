﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ngchat.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ngchat.Hubs;
using ngchat.Services.Messages;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using ngchat.Services.OnlineStatus;
using Microsoft.AspNetCore.SignalR;

namespace ngchat {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.Configure<CookiePolicyOptions>(options => {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddTransient<UserManager<IdentityUser>>();

            services.AddTransient<IMessagesStorage, AzureMessageStorage>(sp => {
                var tableClient = CloudStorageAccount.Parse(Configuration.GetConnectionString("TableStorageConnectionString")).CreateCloudTableClient();
                var cloudTable = tableClient.GetTableReference("chats");
                cloudTable.CreateIfNotExistsAsync();//todo: no need to call it every time
                return new AzureMessageStorage(sp.GetService<UserManager<IdentityUser>>(), cloudTable);
            });

            services.AddTransient<IOnlineStorage, AzureOnlineStorage>(sp => {
                var tableClient = CloudStorageAccount.Parse(Configuration.GetConnectionString("TableStorageConnectionString")).CreateCloudTableClient();
                var cloudTable = tableClient.GetTableReference("online");
                cloudTable.CreateIfNotExistsAsync();//todo: no need to call it every time
                return new AzureOnlineStorage(sp.GetService<UserManager<IdentityUser>>(), cloudTable);
            });

            services.AddSignalR();
            
            services.AddHostedService<ServerOnlineNotificationTimer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSignalR(routes => {
                routes.MapHub<CommonChatHub>("/commonChatHub");
            });

        }
    }
}
