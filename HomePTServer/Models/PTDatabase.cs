using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomePTServer.Models
{
    public static class PTDatabase
    {
        public static PTLocalPatient GetPatient(string email, string password) {
            // get the patient with this email and password, or return null if it doesn't exist
            return null;
        }

        public static PTLocalPatient CreatePatient(PTLocalPatient patient) {
            // add the patient to the db, set patient.ID, and return the patient
            return patient;
        }

        public static List<PTProtocol> GetProtocolsForPatient(int patientID) {
            return null;
        }

        public static List<PTMessage> GetMessagesForPatient(int patientID, int afterTime) {
            return null;
        }

        public static string PathForImageNamed(string fileName) {
            return "/images/directory/" + fileName;
        }

        public static PTMessage AddMessage(PTMessage message, byte[] imageData) {
            // add the message to the db, set message.ID, and return the message
            return message;
        }

        public static PTExerciseProgress AddProgress(PTExerciseProgress progress) {
            // add the progress to the db, set progress.ID, and return the progress
            return progress;
        }
    }
}