using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomePTServer.Models
{
    public class PTProtocol
    {
        public int ID { get; set; }
        public string name { get; set; }
        public List<PTExercise> exercises { get; set; }
        public PTDoctor doctor { get; set; }
        public string imageURL { get; set; }
    }
}