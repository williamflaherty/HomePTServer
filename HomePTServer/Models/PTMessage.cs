using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomePTServer.Models
{

    public class PTMessage
    {
        public int ID { get; set; }
        public string type { get; set; }
        public string text { get; set; }
        public int protocolID { get; set; }
        public int exerciseID { get; set; }
        public int senderID { get; set; }
        public string imageName { get; set; }
        public int timestamp { get; set; }
    }
}