using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomePTServer.Models
{
    public class PTExerciseProgress
    {
        public int ID { get; set; }
        public double timestamp { get; set; }
        public int value { get; set; }
        public int exerciseID { get; set; }
    }
}