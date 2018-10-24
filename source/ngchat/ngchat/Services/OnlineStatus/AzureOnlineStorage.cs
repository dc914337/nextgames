using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ngchat.Models;
using ngchat.Models.AzureTableModels;

namespace ngchat.Services.OnlineStatus {
    public class AzureOnlineStorage : IOnlineStorage {
        const int THRESHOLD_ONLINE_SECONDS = 5; // how long do we consider user to be alive after he was active
        public AzureOnlineStorage(
            UserManager<IdentityUser> usersManager,
            CloudTable onlineTable) {
            UsersManager = usersManager;
            OnlinePresenseTable = onlineTable;
        }
        public UserManager<IdentityUser> UsersManager { get; }
        public CloudTable OnlinePresenseTable { get; }
        public string ContextName { get; } //context in which we want to track the users. I.E. if we want to keep different online lists

        public async Task<IEnumerable<UserContract>> GetOnlineUsersAsync(DateTime forTime) {
            var query = new TableQuery<Models.AzureTableModels.OnlineStatus>().
                Where(TableQuery.GenerateFilterConditionForDate(
                    "Timestamp",
                    QueryComparisons.GreaterThanOrEqual,
                    forTime.AddSeconds(-THRESHOLD_ONLINE_SECONDS)));

            //todo: extract into a generic method
            var results = new List<Models.AzureTableModels.OnlineStatus>();
            TableContinuationToken continuationToken = null;
            do {
                TableQuerySegment<Models.AzureTableModels.OnlineStatus> queryResults;
                try {
                    queryResults = await OnlinePresenseTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                } catch (StorageException ex) {
                    return new List<UserContract>();
                }
                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results);
            } while (continuationToken != null);
            if (!results.Any())
                return new List<UserContract>();
            return results.GroupBy(a => a.UserId).Select(a => new UserContract {
                UserGUID = a.First().UserId,
                Username = UsersManager.FindByIdAsync(a.First().UserId).Result.UserName
            });
        }
        public async Task<bool> RegisterActivityAsync(UserContract pinged, DateTime time, string presenseContextId) {
            var insertOperation = TableOperation.InsertOrReplace(new Models.AzureTableModels.OnlineStatus() {
                PresenseContext = presenseContextId,
                UserId = pinged.UserGUID,
                Timestamp = time.ToUniversalTime()
            });
            TableResult res;
            try {
                res = await OnlinePresenseTable.ExecuteAsync(insertOperation);
            } catch (StorageException ex) {
                return false;
            }
            return true; //todo: check status code
        }

    }
}
