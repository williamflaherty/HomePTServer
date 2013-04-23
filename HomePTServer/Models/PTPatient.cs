using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomePTServer.Models
{
    public class PTPatient
    {
        public int ID { get; set; }
        public bool isActive { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string birthday { get; set; }
        public string email { get; set; }
        public int recentMessageCount { get; set; }
        public List<PTProtocol> protocols { get; set; }
        
        public PTPatient()
        {
        }
    }
}