using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ngchat.Models.AzureTableModels {
    public class OnlineStatus : TableEntity {
        private string _presenseContext;
        private string _userId;

        public string PresenseContext {
            get => _presenseContext;
            set {
                this.PartitionKey = value;
                _presenseContext = value;
            }
        }

        private DateTime TimeStamp { get; set; }
        public string UserId {
            get => _userId; set {
                this.RowKey = value;
                _userId = value;
            }
        }

    }
}
