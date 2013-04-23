using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomePTServer.Models
{
    //TODO: this is a terrible way to have done this.
    public class PTProtocolTemplate
    {
        public int ID { get; set; }
        public List<PTExercise> exercises { get; set; }
        public bool isAssigned { get; set; }
    }
}