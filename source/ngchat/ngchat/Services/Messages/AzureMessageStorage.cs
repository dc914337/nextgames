using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ngchat.Models;
using ngchat.Models.AzureTableModels;

namespace ngchat.Services.Messages {
    public class AzureMessageStorage : IMessagesStorage {

        public AzureMessageStorage(
            UserManager<IdentityUser> usersManager,
            CloudTable messagesTable) {
            UsersManager = usersManager;
            MessagesTable = messagesTable;
        }

        public UserManager<IdentityUser> UsersManager { get; }
        public CloudTable MessagesTable { get; }
        public string ChatName { get; }

        async Task<IEnumerable<MessageContract>> IMessagesStorage.GetHistoryAsync(DateTime from) {
            var query = new TableQuery<Message>().
                Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, from.ToFileTime().ToString()));

            //todo: extract into a generic method
            var results = new List<Message>();
            TableContinuationToken continuationToken = null;
            do {
                var queryResults = await MessagesTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results);
            } while (continuationToken != null);
            return results.Select(a => new MessageContract {
                ChatId = a.ChatId,
                Created = a.Created,
                Message = a.Text,
                Sender = new UserContract {
                    UserGUID = a.UserId,
                    Username = a.Username
                }
            });
        }
        async Task<bool> IMessagesStorage.SaveMessageAsync(MessageContract message) {
            TableOperation insertOperation = TableOperation.InsertOrReplace(new Message() {
                Created = message.Created,
                Text = message.Message,
                UserId = message.Sender.UserGUID,
                Username = message.Sender.Username,
                ChatId = message.ChatId
            });
            TableResult res;
            try {
                res = await MessagesTable.ExecuteAsync(insertOperation);
            } catch (StorageException ex) {
                return false;
            }
            return true; //todo: check status code
        }
    }
}
