﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomePTServer.Models
{
    public class PTLocalPatient
    {
        public int ID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string pushtoken { get; set; }

        public PTLocalPatient()
        {
        }
    }
}