using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace HomePTServer.Models
{
    public static class PTDatabase
    {
        #region Patient
        public static PTLocalPatient GetPatient(string email, string password)
        {
            // get the patient with this email and password, or return null if it doesn't exist
            using (PTLinkDatabaseDataContext db = new PTLinkDatabaseDataContext())
            {
                PTLocalPatient patient = new PTLocalPatient();
                var dbPatient = (from p in db.Persons
                                 where p.User.Email == email
                                 && p.User.Password == password
                                 && p.Permissions == "Patient"
                                 && p.User.RegistrationStatus.Status == "Confirmed"
                                 select p).SingleOrDefault();

                patient.email = email;
                patient.firstName = dbPatient.FirstName;
                patient.lastName = dbPatient.LastName;
                patient.ID = dbPatient.Id;
                patient.password = password;
                patient.pushtoken = dbPatient.PushToken;

                return patient;
            }
        }

        public static PTLocalPatient CreatePatient(PTLocalPatient patient)
        {
            //TODO: if user does not yet exist, we add the user, but it probably shouldn't add with status as confirmed.
            // add the patient to the db, set patient.ID, and return the patient
            using (PTLinkDatabaseDataContext db = new PTLinkDatabaseDataContext())
            {
                Person person = new Person();
                User user = (from u in db.Users
                             where u.Email == patient.email
                             && u.Password == patient.password
                             select u).SingleOrDefault();

                person.FirstName = patient.firstName;
                person.LastName = patient.lastName;
                person.Permissions = "Patient";
                person.PushToken = patient.pushtoken;
                person.IsActive = true;

                if (user == null)
                {
                    user = new User();
                    user.Email = patient.email;
                    user.Password = patient.password;
                    user.RegistrationStatus = (from r in db.RegistrationStatus
                                               where r.Status == "Confirmed"
                                               select r
                                               ).SingleOrDefault();
                    user.Persons.Add(person);
                    db.Users.InsertOnSubmit(user);
                    db.SubmitChanges();
                }
                else
                {
                    user.Persons.Add(person);
                    db.SubmitChanges();
                }

                patient.ID = user.Persons.SingleOrDefault().Id;
                return patient;
            }
        }

        public static List<PTProtocol> GetProtocolsForPatient(int patientID)
        {
            //Get all assigned protocols for a patient
            //TODO: needs testing
            //TODO: assumes only one doctor
            using (PTLinkDatabaseDataContext db = new PTLinkDatabaseDataContext())
            {
                List<PTProtocol> protocols = new List<PTProtocol>();
                PTProtocol protocol;
                PTExercise exercise;
                PTExerciseProgress progress;

                var dbProtocols = (from proto in db.Protocols
                                   where proto.PatientId == patientID
                                   select proto);

                if (dbProtocols != null)
                {
                    foreach (Protocol dbProtocol in dbProtocols)
                    {
                        protocol = new PTProtocol();
                        protocol.ID = dbProtocol.Id;
                        protocol.exercises = new List<PTExercise>();
                        protocol.doctor = new PTDoctor();
                        var dbDoctor = (from protoDXref in db.ProtocolDoctorXrefs
                                        join d in db.Persons on protoDXref.DoctorId equals d.Id
                                        where protoDXref.ProtocolId == protocol.ID
                                        select d).FirstOrDefault();
                        if (dbDoctor.Birthday.HasValue)
                        {
                            protocol.doctor.birthday = dbDoctor.Birthday.Value.ToShortDateString();
                        }
                        if (dbDoctor.Division != null)
                        {
                            protocol.doctor.division = dbDoctor.Division.Name;
                            if (dbDoctor.Division.Institution != null)
                            {
                                protocol.doctor.institution = dbDoctor.Division.Institution.Name;
                            }
                        }
                        protocol.doctor.email = dbDoctor.User.Email;
                        protocol.doctor.firstName = dbDoctor.FirstName;
                        protocol.doctor.ID = dbDoctor.Id;
                        protocol.doctor.lastName = dbDoctor.LastName;
                        protocol.doctor.middleName = dbDoctor.MiddleName;
                        
                        foreach (Exercise dbExercise in dbProtocol.Exercises)
                        {
                            exercise = new PTExercise();
                            exercise.ID = dbExercise.Id;
                            exercise.exerciseCategory = dbExercise.ExerciseTemplate.CategoryId;
                            exercise.category = dbExercise.ExerciseTemplate.Category.Name;
                            exercise.days = dbExercise.Days;
                            exercise.specialInstructions = dbExercise.SpecialInstruction;
                            exercise.value = dbExercise.Value;

                            if (dbExercise.EndDate.HasValue)
                            {
                                exercise.endTime = dbExercise.EndDate.Value;
                            }

                            if (dbExercise.StartDate.HasValue)
                            {
                                exercise.startTime = dbExercise.StartDate.Value;
                            }

                            //TODO: default values in general? dates?
                            if (dbExercise.TimerDuration.HasValue)
                            {
                                exercise.timerDuration = dbExercise.TimerDuration.Value;
                            }
                            else
                            {
                                exercise.timerDuration = 0;
                            }

                            if (dbExercise.HasTimer.HasValue)
                            {
                                exercise.hasTimer = dbExercise.HasTimer.Value;
                            }
                            else
                            {
                                exercise.hasTimer = false;
                            }

                            if (dbExercise.RepetitionQuantity.HasValue)
                            {
                                exercise.repetitionQuantity = dbExercise.RepetitionQuantity.Value;
                            }
                            else
                            {
                                exercise.repetitionQuantity = 0;
                            }

                            if (dbExercise.SetQuantity.HasValue)
                            {
                                exercise.setQuantity = dbExercise.SetQuantity.Value;
                            }

                            //TODO: ignoring image and stuff for now
                            //tmpExercise.imageURL;
                            //tmpExercise.videoURL;

                            exercise.progress = new List<PTExerciseProgress>();
                            foreach (ExerciseProgressXref dbProgress in dbExercise.ExerciseProgressXrefs)
                            {
                                progress = new PTExerciseProgress();
                                progress.ID = dbProgress.Id;
                                progress.timestamp = dbProgress.Timestamp;
                                progress.value = dbProgress.Progress.Id;
                                exercise.progress.Add(progress);
                            }

                            foreach (Instruction instruction in dbExercise.ExerciseTemplate.Instructions)
                            {
                                exercise.instructions += instruction.Text;
                            }

                            protocol.exercises.Add(exercise);

                        }
                        protocols.Add(protocol);
                    }
                }
                return protocols;
            }
            return null;
        }

        public static List<PTMessage> GetMessages(int protocolID, int afterTime)
        {
            //Gets all messages for either a doctor or a patient (see following TODOs)
            //If you pass an afterTime of 0, it will get the last 30 messages
            //Otherwise, it gets however many messages were sent after afterTime
            //TODO: this really doesn't work in the long run, need a protocol id as well, hence first or default
            //TODO: also, this is assuming there's a one-to-one patient-doctor relationship which is totally false
            using (PTLinkDatabaseDataContext db = new PTLinkDatabaseDataContext())
            {
                List<PTMessage> messages = new List<PTMessage>();
                PTMessage message;

                var dbMessages = (from p in db.Protocols
                                  where p.Id == protocolID
                                  select p).FirstOrDefault().Messages.Where(l => l.Timestamp > afterTime);

                //if afterTime is 0, only grab the last 30 messages
                if (afterTime == 0)
                {
                    dbMessages = dbMessages.OrderBy(l => l.Timestamp).Take(30);
                }

                foreach (Message dbMessage in dbMessages)
                {
                    message = new PTMessage();
                    if (dbMessage.ExerciseId.HasValue)
                    {
                        message.exerciseID = dbMessage.ExerciseId.Value;
                    }
                    message.ID = dbMessage.Id;
                    message.protocolID = dbMessage.ProtocolId;
                    message.senderID = dbMessage.PersonId;
                    message.timestamp = dbMessage.Timestamp;

                    if (dbMessage.ContentType.Name == "Text")
                    {
                        message.text = dbMessage.Value;
                    }
                    else if (dbMessage.ContentType.Name == "Image")
                    {
                        //TODO: handle images
                        message.imageName = dbMessage.Value;
                    }
                    //TODO: handle video

                    message.type = dbMessage.ContentType.Name;
                    messages.Add(message);
                }
                return messages;
            }
            return null;
        }

        public static string PathForImageNamed(string fileName)
        {
            //TODO: is this relative to server or something on the app?
            return "/images/directory/" + fileName;
        }

        public static PTMessage AddMessage(PTMessage message, byte[] imageData)
        {
            // add the message to the db, set message.ID, and return the message
            //TODO: default return values
            //TODO: handle video...
            using (PTLinkDatabaseDataContext db = new PTLinkDatabaseDataContext())
            {
                Message dbMessage = new Message();
                dbMessage.ContentTypeId = (from c in db.ContentTypes
                                           where c.Name == message.type
                                           select c.Id).SingleOrDefault();
                dbMessage.ExerciseId = message.exerciseID;
                dbMessage.PersonId = message.senderID;
                dbMessage.ProtocolId = message.protocolID;
                dbMessage.Timestamp = (long)message.timestamp;
                
                if (imageData != null)
                {
                    //TODO: handle images and video and such
                    dbMessage.Value = message.imageName;
                }
                else
                {
                    dbMessage.Value = message.text;
                }

                db.Messages.InsertOnSubmit(dbMessage);
                db.SubmitChanges();
                message.ID = dbMessage.Id;
            }
            return message;
        }

        public static PTExerciseProgress AddProgress(PTExerciseProgress progress)
        {
            //TODO: method not going to work without an exercise id
            using (PTLinkDatabaseDataContext db = new PTLinkDatabaseDataContext())
            {
                ExerciseProgressXref dbProgress = new ExerciseProgressXref();
                dbProgress.Timestamp = (long)progress.timestamp;
                dbProgress.ExerciseId = progress.exerciseID;
                dbProgress.ProgressId = progress.value;

                db.ExerciseProgressXrefs.InsertOnSubmit(dbProgress);
                db.SubmitChanges();
                progress.ID = dbProgress.Id;
            }
            return progress;
        }
        #endregion

        #region Doctor
        public static PTDoctor GetDoctor(string email, string password)
        {
            //get the doctor with this email and password, or return null if it doesn't exist
            //TODO: needs testing
            using (PTLinkDatabaseDataContext db = new PTLinkDatabaseDataContext())
            {
                PTDoctor doctor;
                var dbDoctor = (from p in db.Persons
                              where p.User.Email == email
                               && p.User.Password == password
                               && p.Permissions == "Doctor"
                               && p.User.RegistrationStatus.Status == "Confirmed"
                              select p).SingleOrDefault();

                if (dbDoctor != null)
                {
                    doctor = new PTDoctor();
                    if (dbDoctor.Birthday.HasValue)
                    {
                        doctor.birthday = dbDoctor.Birthday.Value.ToShortDateString();
                    }

                    if (dbDoctor.Division != null)
                    {
                        doctor.division = dbDoctor.Division.Name;
                        if (dbDoctor.Division.Institution != null)
                        {
                            doctor.institution = dbDoctor.Division.Institution.Name;
                        }
                    }
                    doctor.firstName = dbDoctor.FirstName;
                    doctor.ID = dbDoctor.Id;
                    doctor.lastName = dbDoctor.LastName;
                    doctor.middleName = dbDoctor.MiddleName;
                    doctor.email = email;
                    return doctor;
                }
            }
            return null;
        }

        public static List<PTPatient> GetPatientsForDoctor(int doctorID, int afterTime)
        {
            //TODO: get active patients for doctor
            //TODO: needs testing
            //TODO: use the decided upon naming convs
            using (PTLinkDatabaseDataContext db = new PTLinkDatabaseDataContext())
            {
                List<PTPatient> assignedPatients = new List<PTPatient>();
                PTPatient tempPatient;

                var patientList = (from doctorProtocols in db.ProtocolDoctorXrefs
                                   join protocols in db.Protocols on doctorProtocols.ProtocolId equals protocols.Id
                                   join patients in db.Persons on protocols.PatientId equals patients.Id
                                   where doctorProtocols.DoctorId == doctorID
                                   && patients.IsActive == true
                                   select patients);

                foreach (Person patient in patientList.ToList())
                {
                    tempPatient = new PTPatient();
                    if (patient.Birthday.HasValue)
                    {
                        tempPatient.birthday = patient.Birthday.Value.ToShortDateString();
                    }

                    tempPatient.email = patient.User.Email;
                    tempPatient.firstName = patient.FirstName;
                    tempPatient.ID = patient.Id;
                    tempPatient.lastName = patient.LastName;
                    tempPatient.middleName = patient.MiddleName;
                    tempPatient.isActive = patient.IsActive;
                    tempPatient.recentMessageCount = patient.Messages.Where(l => l.Timestamp > afterTime).Count();

                    assignedPatients.Add(tempPatient);
                }
                return assignedPatients;
            }
        }

        public static PTPatient GetPatient(int doctorID, int patientID)
        {
            //TODO: get a specific patient with all their protocols, exercises, etc
            //TODO: needs testing
            using (PTLinkDatabaseDataContext db = new PTLinkDatabaseDataContext())
            {
                PTPatient assignedPatient;
                PTProtocol tmpProtocol;
                PTExercise tmpExercise;
                PTExerciseProgress tmpProgress;
                
                var patient = (from doctorProtocols in db.ProtocolDoctorXrefs
                               join protocols in db.Protocols on doctorProtocols.ProtocolId equals protocols.Id
                               join patients in db.Persons on protocols.PatientId equals patients.Id
                               where doctorProtocols.DoctorId == doctorID
                               && patients.Id == patientID
                               select patients).SingleOrDefault();

                if (patient != null)
                {
                    assignedPatient = new PTPatient();

                    if (patient.Birthday.HasValue)
                    {
                        assignedPatient.birthday = patient.Birthday.Value.ToShortDateString();
                    }

                    assignedPatient.email = patient.User.Email;
                    assignedPatient.firstName = patient.FirstName;
                    assignedPatient.ID = patient.Id;
                    assignedPatient.lastName = patient.LastName;
                    assignedPatient.middleName = patient.MiddleName;
                    assignedPatient.recentMessageCount = 0;

                    //Check loads protocols and exercises
                    assignedPatient.protocols = new List<PTProtocol>();
                    foreach (Protocol protocol in patient.Protocols)
                    {
                        tmpProtocol = new PTProtocol();
                        tmpProtocol.ID = protocol.Id;
                        tmpProtocol.exercises = new List<PTExercise>();

                        //TODO: assuming exerciseCategory refers to CategoryId
                        foreach (Exercise exercise in protocol.Exercises)
                        {
                            tmpExercise = new PTExercise();
                            tmpExercise.ID = exercise.Id;
                            tmpExercise.exerciseCategory = exercise.ExerciseTemplate.CategoryId;
                            tmpExercise.category = exercise.ExerciseTemplate.Category.Name;
                            tmpExercise.days = exercise.Days;
                            tmpExercise.specialInstructions = exercise.SpecialInstruction;
                            tmpExercise.value = exercise.Value;

                            if (exercise.EndDate.HasValue)
                            {
                                tmpExercise.endTime = exercise.EndDate.Value;
                            }

                            if (exercise.StartDate.HasValue)
                            {
                                tmpExercise.startTime = exercise.StartDate.Value;
                            }

                            //TODO: default values in general?
                            if (exercise.TimerDuration.HasValue)
                            {
                                tmpExercise.timerDuration = exercise.TimerDuration.Value;
                            }
                            else
                            {
                                tmpExercise.timerDuration = 0;
                            }

                            if (exercise.HasTimer.HasValue)
                            {
                                tmpExercise.hasTimer = exercise.HasTimer.Value;
                            }
                            else
                            {
                                tmpExercise.hasTimer = false;
                            }

                            if (exercise.RepetitionQuantity.HasValue)
                            {
                                tmpExercise.repetitionQuantity = exercise.RepetitionQuantity.Value;
                            }
                            else
                            {
                                tmpExercise.repetitionQuantity = 0;
                            }

                            if (exercise.SetQuantity.HasValue)
                            {
                                tmpExercise.setQuantity = exercise.SetQuantity.Value;
                            }

                            tmpExercise.progress = new List<PTExerciseProgress>();
                            foreach (ExerciseProgressXref progress in exercise.ExerciseProgressXrefs)
                            {
                                tmpProgress = new PTExerciseProgress();
                                tmpProgress.ID = progress.Id;
                                //TODO: what is Z expecting for the timestamp
                                tmpProgress.timestamp = progress.Timestamp;
                                tmpProgress.value = progress.Progress.Id;
                                tmpExercise.progress.Add(tmpProgress);
                            }

                            //TODO: really there could be multiple images and stuff...blergh, just overwrite for now
                            foreach (Instruction instruction in exercise.ExerciseTemplate.Instructions)
                            {
                                tmpExercise.instructions += instruction.Text;
                                tmpExercise.imageURL = instruction.InstructionImages.First().ImagePath;
                                tmpExercise.videoURL = instruction.VideoPath;
                            }

                            tmpProtocol.exercises.Add(tmpExercise);
                        }
                        assignedPatient.protocols.Add(tmpProtocol);
                    }
                    return assignedPatient;
                }
            }

            return null;
        }

        public static List<PTProtocolTemplate> GetProtocolsForDoctor(int doctorID, int? patientID)
        {
            //TODO: describe what this does
            //TODO: needs testing
            //TODO: fetching custom names and stuff...
            using (PTLinkDatabaseDataContext db = new PTLinkDatabaseDataContext())
            {
                List<PTProtocolTemplate> allowedProtocolTemplates;
                PTProtocolTemplate tempProtocolTemplate;
                PTExercise tmpExercise;
                Exercise protocolTemplateExercise;
                int tmpValue;

                var protocolTemplates = (from protoTXref in db.ProtocolTemplateDoctorXrefs
                                         where protoTXref.DoctorId == doctorID
                                         select protoTXref.ProtocolTemplate);

                if (protocolTemplates != null)
                {
                    allowedProtocolTemplates = new List<PTProtocolTemplate>();

                    foreach (ProtocolTemplate protocolTemplate in protocolTemplates)
                    {
                        tempProtocolTemplate = new PTProtocolTemplate();
                        tempProtocolTemplate.ID = protocolTemplate.Id;
                        protocolTemplateExercise = (from eTXref in db.ExerciseTemplateDoctorXrefs
                                                    join e in db.Exercises on eTXref.ExerciseTemplateId equals e.ExerciseTemplateId
                                                    where e.Protocol.PatientId == patientID
                                                    && eTXref.DoctorId == doctorID
                                                    && e.ProtocolTemplateId == protocolTemplate.Id
                                                    select e).FirstOrDefault();

                        if (protocolTemplateExercise != null)
                        {
                            tempProtocolTemplate.isAssigned = true;
                        }

                        tempProtocolTemplate.exercises = new List<PTExercise>();
                        //TODO: assuming exerciseCategory refers to CategoryId
                        foreach (Exercise exercise in protocolTemplate.Exercises)
                        {
                            //TODO: change to use () stuff
                            if (exercise.ProtocolId == null)
                            {
                                tmpExercise = new PTExercise();
                                tmpExercise.ID = exercise.Id;
                                tmpExercise.exerciseCategory = exercise.ExerciseTemplate.CategoryId;
                                tmpExercise.category = exercise.ExerciseTemplate.Category.Name;
                                tmpExercise.days = exercise.Days;
                                tmpExercise.specialInstructions = exercise.SpecialInstruction;
                                tmpExercise.value = exercise.Value;

                                if (exercise.EndDate.HasValue)
                                {
                                    //TODO: check date conversion stuff
                                    //TODO: default value?
                                    tmpExercise.endTime = exercise.EndDate.Value;
                                }

                                if (exercise.StartDate.HasValue)
                                {
                                    //TODO: check date conversion stuff
                                    //TODO: default value?
                                    tmpExercise.startTime = exercise.StartDate.Value;
                                }

                                //TODO: default values in general?
                                if (exercise.TimerDuration.HasValue)
                                {
                                    tmpExercise.timerDuration = exercise.TimerDuration.Value;
                                }
                                else
                                {
                                    tmpExercise.timerDuration = 0;
                                }

                                if (exercise.HasTimer.HasValue)
                                {
                                    tmpExercise.hasTimer = exercise.HasTimer.Value;
                                }
                                else
                                {
                                    tmpExercise.hasTimer = false;
                                }

                                if (exercise.RepetitionQuantity.HasValue)
                                {
                                    tmpExercise.repetitionQuantity = exercise.RepetitionQuantity.Value;
                                }
                                else
                                {
                                    tmpExercise.repetitionQuantity = 0;
                                }

                                if (exercise.SetQuantity.HasValue)
                                {
                                    tmpExercise.setQuantity = exercise.SetQuantity.Value;
                                }

                                //TODO: ignoring image and video stuff
                                foreach (Instruction instruction in exercise.ExerciseTemplate.Instructions.ToList())
                                {
                                    tmpExercise.instructions += instruction.Text;
                                    tmpExercise.imageURL = instruction.InstructionImages.First().ImagePath;
                                    tmpExercise.videoURL = instruction.VideoPath;
                                }

                                tempProtocolTemplate.exercises.Add(tmpExercise);
                            }
                        }

                        allowedProtocolTemplates.Add(tempProtocolTemplate);
                    }

                    return allowedProtocolTemplates;
                }
            }

            return null;
        }

        #endregion
    }
}