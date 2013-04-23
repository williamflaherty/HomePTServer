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
    public class DoctorController : Controller
    {
        #region Support
        
        public ActionResult Index()
        {
            return View();
        }

        public class Auth
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        public class PatientArgs
        {
            public int? lastTime { get; set; }
            public int? patientID { get; set; }
            public string message { get; set; }
            public Auth authentication { get; set; } 
        }

        public string GetDoctor(string email, string password)
        {
            PTDoctor d = PTDatabase.GetDoctor(email, password);
            if (d == null) return "Not found";
            return new JavaScriptSerializer().Serialize(d);
        }

        #endregion

        [HttpPost]
        public string Login(Auth authentication)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTDoctor d = PTDatabase.GetDoctor(authentication.email, authentication.password);
            
            if (d == null)
            {
                //results["data"] = doctor;
                results["error"] = "Invalid username or password";
            }
            else
            {
                results["doctor"] = d;
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [HttpPost]
        public string GetPatientsForDoctor(PatientArgs args)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTDoctor d = PTDatabase.GetDoctor(args.authentication.email, args.authentication.password);
            List<PTPatient> patients;
            if (d == null)
            {
                results["error"] = "Invalid username or password";
            }
            else
            {
                if (args.lastTime.HasValue)
                {
                    patients = PTDatabase.GetPatientsForDoctor(d.ID, args.lastTime.Value);
                }
                else
                {
                    //TODO: hardcoded to next year
                    patients = PTDatabase.GetPatientsForDoctor(d.ID, 1398228388);
                }
                if (patients != null)
                {
                    //TODO: check if patient list is empty (none assigned), what it returns
                    results["patients"] = patients;
                }
                else
                {
                    results["error"] = "Invalid username or password";
                }
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [HttpPost]
        public string GetPatientSummary(PatientArgs args)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTDoctor d = PTDatabase.GetDoctor(args.authentication.email, args.authentication.password);
            PTPatient patient;

            if (d == null)
            {
                results["error"] = "Invalid username or password";
            }
            else if(args.patientID.HasValue)
            {
                patient = PTDatabase.GetPatient(d.ID, args.patientID.Value);
                if (patient != null)
                {
                    //TODO: check if is empty what it returns
                    results["patient"] = patient;
                }
                else
                {
                    results["error"] = "No such patient for this doctor.";
                }
            }
            else
            {
                results["error"] = "No such patient for this doctor.";
            }

            return new JavaScriptSerializer().Serialize(results);
        }

        [HttpPost]
        public string GetPatientProtocols(PatientArgs args)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTDoctor d = PTDatabase.GetDoctor(args.authentication.email, args.authentication.password);
            PTPatient patient;

            if (d == null)
            {
                results["error"] = "Invalid username or password";
            }
            else if (args.patientID.HasValue)
            {
                patient = PTDatabase.GetPatient(d.ID, args.patientID.Value);
                if (patient != null)
                {
                    //TODO: check if is empty what it returns
                    results["patientProtocols"] = patient.protocols;
                }
                else
                {
                    results["error"] = "No such patient for this doctor.";
                }
            }
            else
            {
                results["error"] = "No such patient for this doctor.";
            }

            return new JavaScriptSerializer().Serialize(results);
        }

        [HttpPost]
        public string GetAllProtocols(PatientArgs args)
        {
            //Also contains indicator as to whether protocol has been assigned to a given patient
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTDoctor d = PTDatabase.GetDoctor(args.authentication.email, args.authentication.password);
            List<PTProtocolTemplate> protocolTemplates;

            if (d == null)
            {
                results["error"] = "Invalid username or password";
            }
            else if (args.patientID.HasValue)
            {
                protocolTemplates = PTDatabase.GetProtocolsForDoctor(d.ID, args.patientID);
                if (protocolTemplates != null)
                {
                    //TODO: check if is empty what it returns
                    results["protocolTemplates"] = protocolTemplates;
                }
                else
                {
                    results["error"] = "No protocols for this doctor.";
                }
            }
            else
            {
                results["error"] = "No such patient for this doctor.";
            }

            return new JavaScriptSerializer().Serialize(results);
        }

        [HttpPost]
        public string SendMessage(PatientArgs args)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTDoctor d = PTDatabase.GetDoctor(args.authentication.email, args.authentication.password);
            PTMessage message;

            if (d == null)
            {
                results["error"] = "Invalid username or password";
            }
            else if (args.patientID.HasValue)
            {
                //TODO: this should be reworked
                PTMessage messageArg = new PTMessage();
                PTProtocol tempProtocol = PTDatabase.GetProtocolsForPatient(args.patientID.Value).FirstOrDefault();
                messageArg.protocolID = tempProtocol.ID;
                messageArg.exerciseID = tempProtocol.exercises.First().ID;
                messageArg.senderID = d.ID;
                messageArg.text = args.message;
                messageArg.timestamp = args.lastTime.Value;
                messageArg.type = "Text";
                message = PTDatabase.AddMessage(messageArg, null);
                if (message != null)
                {
                    //TODO: check if is empty what it returns
                    //TODO: should it return message
                    //TODO: need to handle images
                    results["message"] = message;
                }
                else
                {
                    results["error"] = "Error adding message to the database.";
                }
            }
            else
            {
                results["error"] = "No such patient for this doctor.";
            }

            return new JavaScriptSerializer().Serialize(results);
        }

        [HttpPost]
        public string GetMessages(PatientArgs args)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            PTDoctor d = PTDatabase.GetDoctor(args.authentication.email, args.authentication.password);
            List<PTMessage> messages;

            if (d == null)
            {
                results["error"] = "Invalid username or password";
            }
            else if (args.patientID.HasValue && args.lastTime.HasValue)
            {
                List<PTProtocol> protocols = PTDatabase.GetProtocolsForPatient(args.patientID.Value);
                if (protocols.Count() > 0)
                {
                    messages = PTDatabase.GetMessages(protocols.FirstOrDefault().ID, args.lastTime.Value);
                    if (messages != null)
                    {
                        results["messages"] = messages;
                    }
                    else
                    {
                        results["error"] = "Error getting messages from the database.";
                    }
                }
                else
                {
                    results["error"] = "No protocol/patient selected.";
                }
            }
            else
            {
                results["error"] = "No such patient for this doctor.";
            }

            return new JavaScriptSerializer().Serialize(results);
        }
    }
}
