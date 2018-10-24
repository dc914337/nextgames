using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ngchat.Models.AzureTableModels {
    public class Message : TableEntity {
        private string _chatId;
        private DateTime _created;

        public Message() { }

        public string UserId { get; set; }

        public string ChatId {
            get => _chatId; set {
                this.PartitionKey = value;
                _chatId = value;
            }
        }
        public DateTime Created {
            get => _created; set {
                this.RowKey = value.ToFileTimeUtc().ToString();
                _created = value;
            }
        }
        public string Text { get; set; }
        public string Username { get; internal set; }
    }
}
