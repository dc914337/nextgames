using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ngchat.Models {
    public class MessageContract {
        public UserContract Sender { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
        public string ChatId { get; set; }
    }
}
