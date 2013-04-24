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
        static JavaScriptSerializer s = new JavaScriptSerializer();
        //
        // GET: /Patient/

        public PatientController() {
            s.MaxJsonLength = Int32.MaxValue;
        }
        public ActionResult Index()
        {
            return View();
        }

        public String GetPatient(string email, string password)
        {
            PTLocalPatient p = PTDatabase.GetPatient(email, password);
            if (p == null) return "Not found";
            return s.Serialize(p);
        }

        public class Auth
        {
            public string email { get; set; }
            public string password { get; set; }
        }
        
        [HttpPost]
        public string Login(PTLocalPatient patient) {
            try {
                SyncArgs args = new SyncArgs();
                args.authentication = new Auth();
                args.authentication.email = patient.email;
                args.authentication.password = patient.password;
                args.lastTime = 0;
                return Sync(args);
            } catch (Exception e) {
                return e.Message;
            }
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
            return s.Serialize(results);
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
            return s.Serialize(results);
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
            string imageName = args.message.imageName;
            if (!String.IsNullOrEmpty(args.imageData)) {
                imageData = Convert.FromBase64String(args.imageData);
            }
            try {
                List<PTMessage> messages = new List<PTMessage>();

                // Add text
                if (!String.IsNullOrEmpty(args.message.text)) {
                    args.message.type = "Text";
                    PTMessage message = PTDatabase.AddMessage(args.message, null);
                    message.imageName = null;
                    if (message != null)
                        messages.Add(message);
                }

                // Add image
                if (imageData != null) {
                    PTMessage imageMessage = new PTMessage();
                    imageMessage.senderID = args.message.senderID;
                    imageMessage.protocolID = args.message.protocolID;
                    imageMessage.timestamp = args.message.timestamp;
                    imageMessage.imageName = imageName;
                    imageMessage.exerciseID = args.message.exerciseID;
                    imageMessage.type = "Image";
                    imageMessage.text = null;

                    imageMessage = PTDatabase.AddMessage(imageMessage, imageData);
                    if (imageMessage != null)
                        messages.Add(imageMessage);
                }
                if (imageData != null) {
                    string path = PTDatabase.PathForImageNamed(imageName, false);
                    System.IO.File.WriteAllBytes(path, imageData);
                }
                if (messages.Count == 0) {
                    results["error"] = "Error adding message to the database.";
                } else {
                    results["messages"] = messages;
                }
            } catch (Exception e) {
                results["error"] = e.Message;
            }
            return s.Serialize(results);
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
            return s.Serialize(results);
        }
    }
}
