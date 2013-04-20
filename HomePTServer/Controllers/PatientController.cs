using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using HomePTServer.Models;
using System.IO;
namespace HomePTServer.Controllers
{
    public class PatientController : Controller
    {
        //
        // GET: /Patient/

        public ActionResult Index()
        {
            return View();
        }

        public class Auth
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        [HttpPost]
        public string Login(PTLocalPatient patient) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTLocalPatient p = PTDatabase.GetPatient(patient.email, patient.password);
            if (p == null) {
                results["error"] = "Invalid username or password";
            } else {
                results["patient"] = p;
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [HttpPost]
        public string CreatePatient(PTLocalPatient patient) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTLocalPatient p = PTDatabase.CreatePatient(patient);
            if (p == null) {
                results["error"] = "Error creating patient.";
            } else {
                results["patient"] = p;
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        public class SyncArgs
        {
            public int lastTime { get; set; }
            public Auth authentication { get; set; }
        }

        [HttpPost]
        public string Sync(SyncArgs args) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTLocalPatient p = PTDatabase.GetPatient(args.authentication.email, args.authentication.password);
            if (p == null) {
                results["error"] = "Invalid username or password.";
            } else {
                results["patient"] = p;
                results["protocols"] = PTDatabase.GetProtocolsForPatient(p.ID);
                results["messages"] = PTDatabase.GetMessagesForPatient(p.ID, args.lastTime);
                Dictionary<string, string> images = new Dictionary<string, string>();
                foreach (PTMessage message in (List<PTMessage>)results["messages"]) {
                    if (!string.IsNullOrEmpty(message.imageName)) {
                        byte[] data = System.IO.File.ReadAllBytes(PTDatabase.PathForImageNamed(message.imageName));
                        if (data != null) {
                            images.Add(message.imageName, Convert.ToBase64String(data));
                        }
                    }
                }
                results["images"] = images;
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        public class AddMessageArgs
        {
            public Auth authentication { get; set; }
            public PTMessage message { get; set; }
            public string imageData { get; set; }
        }

        [HttpPost]
        public string AddMessage(AddMessageArgs args) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            byte[] imageData = null;
            if (!String.IsNullOrEmpty(args.imageData)) {
                imageData = Convert.FromBase64String(args.imageData);
            }
            PTMessage message = PTDatabase.AddMessage(args.message, imageData);
            if (message == null) {
                results["error"] = "Error adding message to the database.";
            } else {
                results["message"] = message;
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        public class AddProgressArgs
        {
            public Auth authentication { get; set; }
            public PTExerciseProgress progress { get; set; }
        }

        [HttpPost]
        public string AddProgress(AddProgressArgs args) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTExerciseProgress progress = PTDatabase.AddProgress(args.progress);
            if (progress == null) {
                results["error"] = "Error adding message to the database.";
            } else {
                results["progress"] = progress;
            }
            return new JavaScriptSerializer().Serialize(results);
        }
    }
}
