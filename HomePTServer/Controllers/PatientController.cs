using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using HomePTServer.Models;

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

        [HttpPost]
        public string Sync(PTLocalPatient patient) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTLocalPatient p = PTDatabase.GetPatient(patient.email, patient.password);
            if (p == null) {
                results["error"] = "Invalid username or password.";
            } else {
                results["patient"] = p;
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [HttpPost]
        public string AddMessage(PTMessage message) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            message = PTDatabase.AddMessage(message);
            if (message == null) {
                results["error"] = "Error adding message to the database.";
            } else {
                results["message"] = message;
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [HttpPost]
        public string AddProgress(PTExerciseProgress progress) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            progress = PTDatabase.AddProgress(progress);
            if (progress == null) {
                results["error"] = "Error adding message to the database.";
            } else {
                results["progress"] = progress;
            }
            return new JavaScriptSerializer().Serialize(results);
        }
    }
}
