using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomePTServer.Models
{
    public class PTDoctor
    {
        public int ID { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string division { get; set; }
        public string institution { get; set; }
        public string birthday { get; set; }
    }
}