using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomePTServer.Models
{
//    @property (nonatomic, readonly) int ID;
//@property (nonatomic, readonly) NSString *email;
//@property (nonatomic, readonly) NSString *firstName, *lastName;
//@property (nonatomic, readonly) NSString *password;

//@property (nonatomic, readonly) NSArray *messages;
//@property (nonatomic, readonly) NSArray *protocols;

    public class PTLocalPatient
    {
        public int ID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public List<PTMessage> messages { get; set; }
        public List<PTProtocol> protocols { get; set; }
    }
}