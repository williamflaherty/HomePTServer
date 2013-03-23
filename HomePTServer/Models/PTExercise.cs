using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomePTServer.Models
{

    public class PTExercise
    {
        public int ID { get; set; }
        public int exerciseType { get; set; }
        public int exerciseCategory { get; set; }
        public bool hasTimer { get; set; }
        public string imageURL { get; set; }
        public string videoURL { get; set; }
        public int days { get; set; }
        public int repetitionQuantity { get; set; }
        public int setQuantity { get; set; }
        public string specialInstructions { get; set; }
        public string instructions { get; set; }
        public int value { get; set; }
        public string category { get; set; }
        public List<PTExerciseProgress> progress { get; set; }
    }
}