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

        public String GetPatient(string email, string password)
        {
            PTLocalPatient p = PTDatabase.GetPatient(email, password);
            if (p == null) return "Not found";
            return new JavaScriptSerializer().Serialize(p);
        }

        public class Auth
        {
            public string email { get; set; }
            public string password { get; set; }
        }
        
        [HttpPost]
        public string Login(PTLocalPatient patient) {
            SyncArgs args = new SyncArgs();
            args.authentication = new Auth();
            args.authentication.email = patient.email;
            args.authentication.password = patient.password;
            args.lastTime = 0;
            return Sync(args);
        }

        [HttpPost]
        public string CreatePatient(PTLocalPatient patient) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            try {
                PTLocalPatient p = PTDatabase.CreatePatient(patient);
                if (p == null) {
                    results["error"] = "Error creating patient.";
                } else {
                    results["patient"] = p;
                }
            } catch (Exception e) {
                results["error"] = e.Message;
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
            try {
                PTLocalPatient p = PTDatabase.GetPatient(args.authentication.email, args.authentication.password);
                if (p == null) {
                    results["error"] = "Invalid username or password.";
                } else {
                    List<PTProtocol> protocols = PTDatabase.GetProtocolsForPatient(p.ID);
                    results["patient"] = p;
                    results["protocols"] = protocols;
                    if (protocols.Count() > 0) {
                        results["messages"] = PTDatabase.GetMessages(protocols.First().ID, args.lastTime);
                        Dictionary<string, string> images = new Dictionary<string, string>();
                        foreach (PTMessage message in (List<PTMessage>)results["messages"]) {
                            if (!string.IsNullOrEmpty(message.imageName)) {
                                try {
                                    byte[] data = System.IO.File.ReadAllBytes(PTDatabase.PathForImageNamed(message.imageName, false));
                                    images.Add(message.imageName, Convert.ToBase64String(data));
                                } catch {

                                }
                            }
                        }
                        results["images"] = images;
                    }
                }
            } catch (Exception e) {
                results["error"] = e.Message;
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
            try {
                PTMessage message = PTDatabase.AddMessage(args.message, imageData);
                if (message != null && imageData != null) {
                    string path = PTDatabase.PathForImageNamed(args.message.imageName, false);
                    System.IO.File.WriteAllBytes(path, imageData);
                }
                if (message == null) {
                    results["error"] = "Error adding message to the database.";
                } else {
                    results["message"] = message;
                }
            } catch (Exception e) {
                results["error"] = e.Message;
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
            try {
                PTExerciseProgress progress = PTDatabase.AddProgress(args.progress);
                if (progress == null) {
                    results["error"] = "Error adding message to the database.";
                } else {
                    results["progress"] = progress;
                }
            } catch (Exception e) {
                results["error"] = e.Message;
            }
            return new JavaScriptSerializer().Serialize(results);
        }
    }
}
